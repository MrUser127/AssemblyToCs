using System.Diagnostics;
using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// Converts stack offsets to constants for easier analysis. (this is taken and edited from https://github.com/SamboyCoding/Cpp2IL/blob/development-gompo-ast/Cpp2IL.Core/Graphs/Analysis/Stack/StackAnalyzer.cs)
/// </summary>
public class StackAnalyzer
{
    [DebuggerDisplay("Size = {Size}")]
    private class StackEntry
    {
        public Stack<bool> State = [];

        public int Size => State.Count;

        public void Push(bool value) => State.Push(value);

        public bool Pop() => State.Pop();

        public StackEntry Copy()
        {
            var newEntry = new StackEntry();
            foreach (var entry in State.Reverse())
                newEntry.Push(entry);
            return newEntry;
        }
    }

    private HashSet<Block> _visited = [];
    private Dictionary<Block, StackEntry> _inComingDelta = [];
    private Dictionary<Block, StackEntry> _outGoingDelta = [];
    private Dictionary<MilInstruction, StackEntry> _instructionsStackState = [];

    private StackAnalyzer()
    {
    }

    public static void Analyze(Method method, Decompiler decompiler)
    {
        var graph = method.FlowGraph!;

        var analyzer = new StackAnalyzer();
        analyzer._inComingDelta[graph.EntryBlock] = new StackEntry();
        var archSize = method.ArchSize;
        analyzer.TraverseGraph(graph.EntryBlock, archSize, graph, decompiler);
        var outDelta = analyzer._outGoingDelta[graph.ExitBlock];

        if (outDelta.State.Count != 0)
            decompiler.Error("Method ends with non empty stack", "Stack Analyzer");

        foreach (var block in graph.Blocks)
        {
            MilInstruction? previousInstruction = null;

            foreach (var instruction in block.Instructions)
            {
                var currentPos = (analyzer._instructionsStackState[instruction].State.Count) * archSize;

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

                var stackState = analyzer._instructionsStackState[callInstruction].State;
                var stackSize = stackState.Count * archSize;
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

        Simplifier.RemoveNops(method);
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

            if (operand is MilMemoryLocation memoryLocation)
            {
                if (memoryLocation.Register != null && memoryLocation.Register > maxRegisterNumber)
                    maxRegisterNumber = (int)memoryLocation.Register;
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
            blockDelta.State.Clear();
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
                        previous.Push(true);
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

                if (expectedDelta.State.Count != blockDelta.State.Count)
                    decompiler.Error("Unbalanced stack", "Stack Analyzer");

                _inComingDelta[succ] = blockDelta;
            }
        }
    }
}
