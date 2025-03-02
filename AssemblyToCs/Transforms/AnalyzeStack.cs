using System.Diagnostics;
using AssemblyToCs.Mil;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Converts stack offsets to registers. (this is taken and edited from https://github.com/SamboyCoding/Cpp2IL/blob/development-gompo-ast/Cpp2IL.Core/Graphs/Analysis/Stack/StackAnalyzer.cs)
/// </summary>
public class AnalyzeStack : ITransform
{
    [DebuggerDisplay("Size = {Count}")]
    private class StackEntry
    {
        public int Count;

        public void Push() => Count++;

        public void Pop() => Count--;

        public StackEntry Copy() => new() { Count = Count };
    }

    private HashSet<Block> _visited = [];
    private Dictionary<Block, StackEntry> _inComingDelta = [];
    private Dictionary<Block, StackEntry> _outGoingDelta = [];
    private Dictionary<MilInstruction, StackEntry> _instructionsStackState = [];

    public void Apply(Method method, Decompiler decompiler)
    {
        if (method.FlowGraph == null)
            throw new NullReferenceException("Control flow graph has not been built!");
        var cfg = method.FlowGraph;

        decompiler.Info("Analyzing stack...", "Stack Analyzer");

        _visited = [];
        _inComingDelta = [];
        _outGoingDelta = [];
        _instructionsStackState = [];

        _inComingDelta[cfg.EntryBlock] = new StackEntry();
        var archSize = method.ArchSize;
        TraverseGraph(cfg.EntryBlock, archSize, cfg, decompiler);
        var outDelta = _outGoingDelta[cfg.ExitBlock];

        if (outDelta.Count != 0)
            decompiler.Error("Method ends with non empty stack", "Stack Analyzer");

        foreach (var block in cfg.Blocks)
        {
            MilInstruction? previousInstruction = null;

            foreach (var instruction in block.Instructions)
            {
                var currentPos = (_instructionsStackState[instruction].Count) * archSize;

                if (instruction.OpCode == MilOpCode.ShiftStack)
                {
                    // nop the shift stack instruction
                    instruction.OpCode = MilOpCode.Nop;
                    instruction.Operands = [];

                    // correct stack offset for previous move instruction if it matches (push/pop combo)
                    if (previousInstruction is
                        { OpCode: MilOpCode.Move, Operands: [MilStackOffset { Offset: 0 }, { } op2] })
                    {
                        previousInstruction.Operands = [new MilStackOffset(currentPos), op2];
                    }
                }

                previousInstruction = instruction;
            }

            // correct offsets for call params
            if (block.IsCall)
            {
                var callInstruction = block.Instructions[^1];

                var stackState = _instructionsStackState[callInstruction].Count;
                var stackSize = stackState * archSize;
                for (var i = 0; i < callInstruction.Operands.Count; i++)
                {
                    var op = callInstruction.Operands[i];
                    if (op is MilStackOffset offset)
                    {
                        var actual = stackSize - offset.Offset;
                        callInstruction.Operands[i] = new MilStackOffset(actual);
                    }
                }
            }
        }

        ReplaceStackWithRegisters(method);
    }

    private static void ReplaceStackWithRegisters(Method method)
    {
        // get all offsets (without duplicates)
        var offsets = new List<int>();
        foreach (var operand in method.Instructions.SelectMany(instruction => instruction.Operands))
        {
            if (operand is MilStackOffset offset)
            {
                if (!offsets.Contains(offset.Offset))
                    offsets.Add(offset.Offset);
            }
        }

        // get max register number
        var maxRegisterNumber = 0;
        foreach (var operand in method.Instructions.SelectMany(instruction => instruction.Operands))
        {
            if (operand is MilRegister register)
            {
                if (register.Number > maxRegisterNumber)
                    maxRegisterNumber = register.Number;
            }
        }

        // map offsets to registers
        var offsetToRegister = new Dictionary<int, int>();
        for (var i = 0; i < offsets.Count; i++)
        {
            var offset = offsets[i];
            offsetToRegister.Add(offset, maxRegisterNumber + i + 1);
        }

        // replace stack offset operands
        foreach (var instruction in method.Instructions)
        {
            for (var i = 0; i < instruction.Operands.Count; i++)
            {
                var operand = instruction.Operands[i];

                if (operand is MilStackOffset offset)
                    instruction.Operands[i] = new MilRegister(offsetToRegister[offset.Offset]);
            }
        }
    }

    private void TraverseGraph(Block block, int archSize, ControlFlowGraph cfg, Decompiler decompiler)
    {
        var blockDelta = _inComingDelta[block].Copy();

        // tail call
        if (block is { IsCall: true, Successors.Count: 1 } && block.Successors[0] == cfg.ExitBlock)
        {
            blockDelta.Count = 0;
            _outGoingDelta[block] = blockDelta;
        }
        else
        {
            var previous = blockDelta;
            foreach (var instruction in block.Instructions)
            {
                _instructionsStackState[instruction] = previous;
                if (instruction.OpCode != MilOpCode.ShiftStack) continue;

                var value = (int)instruction.Operands[0]!;

                if (value % archSize != 0)
                    decompiler.Error($"Unaligned stack shift ({instruction})", "Stack Analyzer");

                previous = previous.Copy();

                // change stack state
                for (var i = 0; i < Math.Abs(value / archSize); i++)
                {
                    if (value < 0)
                        previous.Push();
                    else
                        previous.Pop();
                }
            }

            blockDelta = previous;
            _outGoingDelta[block] = blockDelta;
        }

        // visit successors
        foreach (var succ in block.Successors)
        {
            if (!_visited.Contains(succ))
            {
                _inComingDelta[succ] = blockDelta;
                _visited.Add(succ);
                TraverseGraph(succ, archSize, cfg, decompiler);
            }
            else
            {
                var expectedDelta = _inComingDelta[succ];

                if (expectedDelta.Count != blockDelta.Count)
                    decompiler.Error("Unbalanced stack", "Stack Analyzer");

                _inComingDelta[succ] = blockDelta;
            }
        }
    }
}
