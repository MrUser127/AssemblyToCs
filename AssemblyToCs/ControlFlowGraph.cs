using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// Control flow graph for a method.
/// </summary>
public class ControlFlowGraph
{
    /// <summary>
    /// All blocks of the graph.
    /// </summary>
    public List<Block> Blocks = new List<Block>();

    /// <summary>
    /// The entry block.
    /// </summary>
    public Block EntryBlock = new Block();

    /// <summary>
    /// Builds a control flow graph.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <returns>The control flow graph.</returns>
    public static ControlFlowGraph Build(Method method)
    {
        var cfg = new ControlFlowGraph();
        var instructions = method.Instructions;

        if (instructions.Count == 0)
            return cfg;

        MarkBlockStarts(instructions);
        GetBlockEdges(instructions, out var edges);
        cfg.CreateBlocks(instructions);

        foreach (var edge in edges)
            AddEdge(cfg.GetBlockByInstruction(edge.Item1)!, cfg.GetBlockByInstruction(edge.Item2)!);

        if (cfg.Blocks.Count == 0) return cfg;

        // if entry has incoming edges, create new entry
        if (cfg.EntryBlock.Predecessors.Count > 0)
        {
            var newEntry = new Block();
            cfg.Blocks.Insert(0, newEntry);
            cfg.EntryBlock = newEntry;
            AddEdge(newEntry, cfg.Blocks[1]);
        }

        for (var i = 0; i < cfg.Blocks.Count; i++)
            cfg.Blocks[i].Id = i;

        return cfg;
    }

    private static void MarkBlockStarts(List<MilInstruction> instructions)
    {
        var isNextBlockStart = true;

        foreach (var instruction in instructions)
        {
            if (isNextBlockStart)
            {
                instruction.IsBlockStart = true;
                isNextBlockStart = false;
            }

            switch (instruction.OpCode)
            {
                case MilOpCode.Jump:
                case MilOpCode.ConditionalJump:
                    isNextBlockStart = true;
                    GetBranchTarget(instruction).IsBlockStart = true;
                    break;
                case MilOpCode.Return:
                    isNextBlockStart = true;
                    break;
                default:
                    break;
            }
        }
    }

    private static void GetBlockEdges(List<MilInstruction> instructions,
        out List<(MilInstruction, MilInstruction)> edges)
    {
        edges = new List<(MilInstruction, MilInstruction)>();

        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];

            switch (instruction.OpCode)
            {
                case MilOpCode.Jump:
                    edges.Add((instruction, GetBranchTarget(instruction)));
                    break;
                case MilOpCode.ConditionalJump:
                    if (i < instructions.Count - 1)
                        edges.Add((instruction, instructions[i + 1]));
                    edges.Add((instruction, GetBranchTarget(instruction)));
                    break;
                case MilOpCode.Return:
                    break;
                default:
                    if (i < instructions.Count - 1 && instructions[i + 1].IsBlockStart)
                        edges.Add((instruction, instructions[i + 1]));
                    break;
            }
        }
    }

    private void CreateBlocks(List<MilInstruction> instructions)
    {
        var block = EntryBlock;
        Blocks.Add(block);

        block.AddInstruction(instructions[0]);

        for (var i = 1; i < instructions.Count; i++)
        {
            var instruction = instructions[i];

            if (instruction.IsBlockStart)
            {
                block = new Block();
                Blocks.Add(block);
            }

            block.AddInstruction(instruction);
        }
    }

    private static void AddEdge(Block from, Block to)
    {
        from.Successors.Add(to);
        to.Predecessors.Add(from);
    }

    private static MilInstruction GetBranchTarget(MilInstruction instruction) =>
        (MilInstruction)instruction.Operands[0]!;

    private Block? GetBlockByInstruction(MilInstruction instruction) =>
        Blocks.FirstOrDefault(block => block.Instructions.Any(i => i == instruction));
}
