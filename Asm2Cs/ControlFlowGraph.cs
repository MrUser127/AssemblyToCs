using Asm2Cs.IL;

namespace Asm2Cs;

/// <summary>
/// Control flow graph.
/// </summary>
public class ControlFlowGraph
{
    /// <summary>
    /// All blocks that make up the graph.
    /// </summary>
    public List<Block> Blocks;

    /// <summary>
    /// Entry point of the graph.
    /// </summary>
    public Block EntryBlock;

    /// <summary>
    /// Creates a new control flow graph.
    /// </summary>
    /// <param name="function">The function.</param>
    public ControlFlowGraph(Function function)
    {
        Blocks = new List<Block>();
        EntryBlock = new Block();

        Build(function);
    }

    private void Build(Function function)
    {
        var instructions = function.Instructions;

        Blocks = new List<Block>();

        if (instructions.Count == 0)
            return;

        foreach (var instruction in instructions)
            instruction.IsBlockStart = true;

        GetBlockEdges(instructions, out var blockStarts, out var blockEnds);
        CreateBlocks(instructions);

        for (var i = 0; i < blockStarts.Count; i++)
            AddEdge(GetBlockByInstruction(blockStarts[i])!, GetBlockByInstruction(blockEnds[i])!);

        foreach (var instruction in instructions)
        {
            if (instruction.OpCode is not (ILOpCode.Jump or ILOpCode.ConditionalJump or ILOpCode.IndirectJump))
                continue;

            if (instruction.OpCode == ILOpCode.IndirectJump)
            {
                function.AddComment($"Indirect jumps are not implemented: {instruction}", instruction);
                continue;
            }

            var target = GetBranchTarget(instruction);
            ((BranchTarget)instruction.Operands[0]).Block = GetBlockByInstruction(target);
        }

        if (Blocks.Count <= 0) return;
        if (EntryBlock.Predecessors.Count <= 0) return;

        var newEntry = new Block();
        Blocks.Insert(0, newEntry);
        EntryBlock = newEntry;
        AddEdge(newEntry, Blocks[1]);
    }

    private void GetBlockEdges(List<ILInstruction> instructions, out List<ILInstruction> blockStarts,
        out List<ILInstruction> blockEnds)
    {
        blockStarts = new List<ILInstruction>();
        blockEnds = new List<ILInstruction>();

        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            var nextIsBlockStart = i == instructions.Count - 1 || instruction.IsBlockStart;
            int currentInstructionIndex;

            switch (instruction.OpCode)
            {
                case ILOpCode.Jump:
                    blockStarts.Add(instruction);
                    blockEnds.Add(GetBranchTarget(instruction));
                    break;
                case ILOpCode.ConditionalJump:
                    blockStarts.Add(instruction);
                    currentInstructionIndex = instructions.FindIndex(instruction2 => instruction2 == instruction);
                    blockEnds.Add(instructions[currentInstructionIndex + 1]);
                    blockStarts.Add(instruction);
                    blockEnds.Add(GetBranchTarget(instruction));
                    break;
                case ILOpCode.IndirectJump:
                    break; // Rebuild() adds warning about this
                case ILOpCode.Return:
                    break;
                default:
                    if (nextIsBlockStart)
                    {
                        blockStarts.Add(instruction);
                        currentInstructionIndex = instructions.FindIndex(instruction2 => instruction2 == instruction);
                        blockEnds.Add(instructions[currentInstructionIndex + 1]);
                    }

                    break;
            }
        }
    }

    private void CreateBlocks(List<ILInstruction> instructions)
    {
        Block block = new Block { ID = Blocks.Count };
        Blocks.Add(block);
        EntryBlock = block;

        block.AddInstruction(instructions[0]);

        for (var i = 1; i < instructions.Count; i++)
        {
            var instruction = instructions[i];

            if (instruction.IsBlockStart)
            {
                block = new Block { ID = Blocks.Count };
                Blocks.Add(block);
            }

            block.AddInstruction(instruction);
        }
    }

    private static ILInstruction GetBranchTarget(ILInstruction instruction) =>
        ((BranchTarget)instruction.Operands[0]).Instruction;

    private Block? GetBlockByInstruction(ILInstruction? instruction)
    {
        if (instruction == null)
            return null;

        foreach (var block in Blocks)
            if (block.Instructions.Any(blockInstruction => blockInstruction == instruction))
                return block;

        return null;
    }

    private static void AddEdge(Block from, Block to)
    {
        from.Successors.Add(to);
        to.Predecessors.Add(from);
    }
}
