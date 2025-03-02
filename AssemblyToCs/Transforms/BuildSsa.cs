using AsmResolver.DotNet.Signatures;
using AssemblyToCs.Mil;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Builds SSA (static single assignment) form for a method.
/// </summary>
public class BuildSsa : ITransform
{
    private Dictionary<MilLocalVariable, Stack<MilLocalVariable>> _versions = new();
    private Dictionary<MilLocalVariable, int> _versionCount = new();

    public void Apply(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        decompiler.Info("Building SSA form...", "Build SSA");
        ReplaceRegistersWithLocals(method);
        ConvertToSsa(method);
        decompiler.Info("Inserting phi functions...", "Build SSA");
        AddLocalsToMethod(method);
        ReplaceBranchTargetsWithBlocks(
            method); // InsertPhiFunctions adds instructions so this needs to be done so that branches don't point to the second instruction in blocks
        InsertPhiFunctions(method);
        ReplaceBlocksWithBranchTargets(method);
        AddLocalsToMethod(method);
    }

    private void InsertPhiFunctions(Method method)
    {
        var cfg = method.FlowGraph!;

        // find where locals are assigned
        var defs = new Dictionary<MilLocalVariable, Block>();
        foreach (var block in cfg.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                if (instruction.IsAssignment && instruction.Operands[0] is MilLocalVariable local)
                {
                    /*if (!defs.ContainsKey(local))
                        defs[local] = [];*/

                    defs[local] = block;
                }
            }
        }

        // check where phi functions should be
        var phiFunctions = new Dictionary<Block, List<(MilLocalVariable, List<MilLocalVariable>)>>();

        // this is so that i don't need to do weird stuff to add to value in _versionCount
        var versionNames = _versionCount.Select(kv => kv.Key.Name).ToList();
        var versionCount = _versionCount.Select(kv => kv.Value).ToList();

        var defsKeys = defs.Keys.ToList();
        var defsValues = defs.Values.ToList();

        foreach (var (varName, definingBlocks) in defs)
        {
            var workList = new Queue<Block>(new[] { definingBlocks });
            var placed = new HashSet<Block>();

            while (workList.Count > 0)
            {
                var block = workList.Dequeue();

                foreach (var frontierBlock in method.Dominance!.DominanceFrontier[block])
                {
                    if (!placed.Add(frontierBlock))
                        continue;

                    var phiOperands = frontierBlock.Predecessors
                        .Select(pred => defsKeys[defsValues.FindIndex(b => b == pred)])
                        .ToList();

                    if (!phiFunctions.ContainsKey(frontierBlock))
                        phiFunctions[frontierBlock] = [];

                    var index = versionNames.FindIndex(v => v == varName.Name);
                    var newLocal = varName.Copy(versionCount[index]);
                    versionCount[index]++;

                    phiFunctions[frontierBlock].Add((newLocal, phiOperands));

                    workList.Enqueue(frontierBlock);
                }
            }
        }

        // add phi functions
        foreach (var (block, phis) in phiFunctions)
        {
            foreach (var (dest, sources) in phis)
            {
                var operands = new List<object> { dest }.Concat(sources).ToArray();
                var phiInstr = new MilInstruction(0, MilOpCode.Phi, operands);

                // add that instruction
                var index = method.Instructions.FindIndex(i => i == block.Instructions[0]);
                block.Instructions.Insert(0, phiInstr);
                method.Instructions.Insert(index, phiInstr);

                ReplaceLocalsUntilReassignment(block, 1, dest); // replace with result from phi
            }
        }

        // instructions are added so indexes need to be fixed
        for (var i = 0; i < method.Instructions.Count; i++)
            method.Instructions[i].Index = i;
    }

    private static void ReplaceLocalsUntilReassignment(Block block, int startIndex, MilLocalVariable local)
    {
        for (var i = startIndex; i < block.Instructions.Count; i++)
        {
            var instruction = block.Instructions[i];

            // reassignment?
            if (instruction.IsAssignment)
            {
                if ((MilLocalVariable)instruction.Destination! == local)
                    return;
            }

            // replace it
            for (var j = 0; j < instruction.Operands.Count; j++)
            {
                var operand = instruction.Operands[j];

                if (operand is MilLocalVariable local2)
                {
                    if (local2.Name == local.Name)
                        instruction.Operands[j] = local;
                }
            }
        }
    }

    private static void ReplaceBranchTargetsWithBlocks(Method method)
    {
        foreach (var instruction in method.Instructions)
        {
            for (var i = 0; i < instruction.Operands.Count; i++)
            {
                var operand = instruction.Operands[i];

                if (operand is MilInstruction branchInstruction)
                {
                    instruction.Operands[i] = method.FlowGraph!.GetBlockByInstruction(branchInstruction)!;
                }
            }
        }
    }

    private static void ReplaceBlocksWithBranchTargets(Method method)
    {
        foreach (var instruction in method.Instructions)
        {
            for (var i = 0; i < instruction.Operands.Count; i++)
            {
                var operand = instruction.Operands[i];

                if (operand is Block block)
                {
                    instruction.Operands[i] = method.Instructions.FindIndex(i => i == block.Instructions[0]);
                }
            }
        }
    }

    private static void AddLocalsToMethod(Method method)
    {
        List<MilLocalVariable> locals = [];
        foreach (var operand in method.Instructions.SelectMany(instruction => instruction.Operands))
        {
            if (operand is MilLocalVariable local)
            {
                if (!locals.Contains(local))
                    locals.Add(local);
            }
        }

        method.Locals = locals;
    }

    private static void ReplaceRegistersWithLocals(Method method)
    {
        // get all registers (without duplicates)
        var registers = new List<int>();
        foreach (var operand in method.Instructions.SelectMany(instruction => instruction.Operands))
        {
            if (operand is MilRegister register)
            {
                if (!registers.Contains(register.Number))
                    registers.Add(register.Number);
            }
        }

        // also params
        foreach (var parameter in method.Parameters)
        {
            if (parameter is MilRegister register)
            {
                if (!registers.Contains(register.Number))
                    registers.Add(register.Number);
            }
        }

        // map registers to locals
        var locals = new Dictionary<int, MilLocalVariable>();
        foreach (var register in registers)
            locals.Add(register, new MilLocalVariable($"v{register}", register, null, 0) { Register = register });

        // replace registers with locals
        foreach (var instruction in method.Instructions)
        {
            for (var i = 0; i < instruction.Operands.Count; i++)
            {
                var operand = instruction.Operands[i];

                if (operand is MilRegister register)
                    instruction.Operands[i] = locals[register.Number];
            }
        }

        for (var i = 0; i < method.Parameters.Count; i++)
        {
            var parameter = method.Parameters[i];
            if (parameter is MilRegister register)
                method.Parameters[i] = locals[register.Number];
        }
    }

    private void ConvertToSsa(Method method)
    {
        if (method.FlowGraph == null)
            throw new NullReferenceException("Control flow graph has not been built!");
        var cfg = method.FlowGraph;

        if (method.Dominance == null)
            throw new NullReferenceException("Dominance info has not been built!");
        var dominanceTree = method.Dominance.DominanceTree;

        _versions.Clear();
        _versionCount.Clear();

        // initial variables (version 0)
        foreach (var block in cfg.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                foreach (var operand in instruction.Operands)
                {
                    if (operand is not MilLocalVariable local) continue;

                    if (!_versions.ContainsKey(local))
                    {
                        _versions.Add(local, new Stack<MilLocalVariable>());
                        // params are version 1
                        _versionCount.Add(local, 2);
                        _versions[local].Push(new MilLocalVariable($"v{local}", local.Register, null, 1));
                    }
                }
            }
        }

        ProcessBlock(cfg.EntryBlock, dominanceTree);
    }

    private void GetNewVariable(MilLocalVariable old, out MilLocalVariable newVariable)
    {
        newVariable = old.Copy(_versionCount[old]);
        _versions[old].Push(newVariable);
        _versionCount[old]++;
    }

    private void ProcessBlock(Block block, Dictionary<Block, List<Block>> dominanceTree)
    {
        foreach (var instruction in block.Instructions)
        {
            // create new version
            if (instruction.IsAssignment)
            {
                var destination = (MilLocalVariable)instruction.Destination!;
                GetNewVariable(destination, out var newName);
                instruction.Destination = newName;
            }

            // replace locals with ssa versions
            for (var i = 0; i < instruction.Operands.Count; i++)
            {
                if (instruction.Operands[i] is not MilLocalVariable local) continue;

                if (_versions.TryGetValue(local, out var versions))
                    instruction.Operands[i] = local.Copy(versions.Peek().Version);
            }
        }

        // process children in the tree
        if (dominanceTree.TryGetValue(block, out var children))
        {
            foreach (var child in children)
                ProcessBlock(child, dominanceTree);
        }

        // remove locals from versions but not from count
        foreach (var instruction in block.Instructions.Where(instr => instr.IsAssignment))
        {
            var local = (MilLocalVariable)instruction.Destination!;
            _versions.FirstOrDefault(kv => kv.Key.Name == local.Name).Value.Pop();
        }
    }
}
