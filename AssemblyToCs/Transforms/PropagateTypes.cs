using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AssemblyToCs.Mil;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Performs type propagation on methods.
/// </summary>
public class PropagateTypes : ITransform
{
    private Dictionary<MilLocalVariable, TypeSignature> _types = [];
    private CorLibTypeFactory _corLibTypes;

    public void Apply(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        foreach (var local in method.Locals)
            local.Type = corLibTypes.Object;
        return;

        // TODO: i can't get this to work

        if (method.FlowGraph == null)
            throw new NullReferenceException("Control flow graph has not been built!");

        if (method.Dominance == null)
            throw new NullReferenceException("Dominance info has not been built!");

        decompiler.Info("Propagating types...", "Propagate Types");
        var cfg = method.FlowGraph;
        var dominance = method.Dominance;

        _corLibTypes = corLibTypes;
        _types = [];

        // get parameter types
        foreach (var parameter in method.Parameters)
        {
            if (parameter is MilLocalVariable local)
                _types[local] = local.Type!;
        }

        PropagateFromReturnType(method);
        Propagate(method, cfg, dominance);
    }

    private TypeSignature MergeTypes(TypeSignature a, TypeSignature b)
    {
        if (a == b) return a;
        if (a == _corLibTypes.Object) return b;
        if (b == _corLibTypes.Object) return a;

        return _corLibTypes.Object;
    }

    private static void PropagateFromReturnType(Method method)
    {
        var returns = method.Instructions.Where(i => i.OpCode == MilOpCode.Return).ToList();

        foreach (var returnInstruction in returns)
        {
            if (returnInstruction.Operands[0] == null) continue;

            var local = (MilLocalVariable)returnInstruction.Operands[0]!;
            local.Type = method.Definition.Parameters.ReturnParameter.ParameterType;
        }
    }

    private static void PropagateFromParameters(Method method)
    {
        foreach (var parameter in method.Parameters)
        {
            if (parameter is not MilLocalVariable parameterLocal) continue;

            foreach (var instruction in method.Instructions)
            {
                if (instruction.OpCode != MilOpCode.Move) continue;

                if (instruction.Operands[0] is not MilLocalVariable destination) continue;
                if (instruction.Operands[1] is not MilLocalVariable source) continue;

                if (parameterLocal.Register == source.Register && parameterLocal.Version == (source.Version - 1))
                    destination.Type = parameterLocal.Type;
            }
        }
    }


    private void Propagate(Method method, ControlFlowGraph cfg, Dominance dominance)
    {
        Queue<Block> workList = [];
        HashSet<Block> visited = [];

        workList.Enqueue(cfg.EntryBlock);
        visited.Add(cfg.EntryBlock);

        while (workList.Count > 0)
        {
            var block = workList.Dequeue();

            foreach (var instruction in block.Instructions)
                PropagateInstruction(instruction);

            // visit dominated blocks
            if (method.Dominance!.DominanceTree.TryGetValue(block, out var dominatedBlocks))
            {
                foreach (var domBlock in dominatedBlocks)
                {
                    if (!visited.Contains(domBlock))
                    {
                        workList.Enqueue(domBlock);
                        visited.Add(domBlock);
                    }
                }
            }
        }
    }

    private void PropagateInstruction(MilInstruction instruction)
    {
        switch (instruction.OpCode)
        {
            case MilOpCode.Move:
                var local = (MilLocalVariable)instruction.Operands[0]!;

                _types[local] = instruction.Operands[1] switch
                {
                    // constant assignment?
                    int => _corLibTypes.Int32,
                    float => _corLibTypes.Single,
                    bool => _corLibTypes.Boolean,
                    string => _corLibTypes.String,
                    _ => /*_types[local]*/ ((MilLocalVariable)instruction.Operands[1]!).Type!
                };
                break;

            case MilOpCode.Add:
            case MilOpCode.Subtract:
            case MilOpCode.Multiply:
            case MilOpCode.Divide:
            case MilOpCode.And:
            case MilOpCode.Or:
            case MilOpCode.Xor:
            case MilOpCode.Not:
            case MilOpCode.Negate:
                // get type from operands
                var local2 = (MilLocalVariable)instruction.Operands[0]!;

                var left = _types[local2];
                var right = _types[(MilLocalVariable)instruction.Operands[1]!];

                if (left == right)
                    _types[local2] = left;
                else
                    _types[local2] = _corLibTypes.Object;

                break;

            case MilOpCode.Call:
                // return type from call

                if (instruction.Operands[0] == null)
                    break;

                var returnValue = (MilLocalVariable)instruction.Operands[0]!;
                var method = (MethodDefinition)instruction.Operands[1]!;
                returnValue.Type = method.Parameters.ReturnParameter.ParameterType;
                break;
            case MilOpCode.Phi:
                var phiResult = (MilLocalVariable)instruction.Operands[0]!;
                var phiSources = instruction.Operands.Skip(1).Cast<MilLocalVariable>().ToList();

                var unifiedType = _types[phiSources[0]];

                foreach (var source in phiSources.Skip(1))
                    unifiedType = MergeTypes(unifiedType, _types[source]);

                _types[phiResult] = unifiedType;

                break;
        }
    }
}
