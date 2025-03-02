using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// Control flow graph for a method. (this is taken and edited from https://github.com/SamboyCoding/Cpp2IL/blob/development/Cpp2IL.Core/Graphs/ISILControlFlowGraph.cs)
/// </summary>
public class ControlFlowGraph
{
    /// <summary>
    /// All blocks of the graph.
    /// </summary>
    public List<Block> Blocks;

    /// <summary>
    /// The entry block.
    /// </summary>
    public Block EntryBlock;

    /// <summary>
    /// The exit block.
    /// </summary>
    public Block ExitBlock;

    private int _nextId;

    /// <summary>
    /// Creates a new control flow graph.
    /// </summary>
    private ControlFlowGraph()
    {
        EntryBlock = new Block() { Id = _nextId++ };
        ExitBlock = new Block() { Id = _nextId++ };
        Blocks = [EntryBlock, ExitBlock];
    }

    /// <summary>
    /// Builds a control flow graph for a method.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="decompiler">The decompiler.</param>
    public static ControlFlowGraph Build(Method method, Decompiler decompiler)
    {
        var cfg = new ControlFlowGraph();

        var instructions = method.Instructions;

        var currentBlock = new Block() { Id = cfg._nextId++ };

        cfg.Blocks.Add(currentBlock);
        AddDirectedEdge(cfg.EntryBlock, currentBlock);

        for (var i = 0; i < instructions.Count; i++)
        {
            var isLast = i == instructions.Count - 1;

            switch (instructions[i].OpCode)
            {
                case MilOpCode.Jump:
                    currentBlock.AddInstruction(instructions[i]);
                    if (!isLast)
                    {
                        var jumpBlock = new Block() { Id = cfg._nextId++ };
                        cfg.Blocks.Add(jumpBlock);
                        if (instructions[i].Operands[0] is MilInstruction)
                            currentBlock.IsDirty = true;
                        else
                            AddDirectedEdge(currentBlock, cfg.ExitBlock);
                        currentBlock = jumpBlock;
                    }
                    else
                    {
                        AddDirectedEdge(currentBlock, cfg.ExitBlock);
                        currentBlock.IsDirty = true;
                    }

                    break;
                case MilOpCode.JumpTrue:
                case MilOpCode.JumpFalse:
                case MilOpCode.JumpEqual:
                case MilOpCode.JumpGreater:
                case MilOpCode.JumpLess:
                    currentBlock.AddInstruction(instructions[i]);
                    if (!isLast)
                    {
                        var condJumpBlock = new Block() { Id = cfg._nextId++ };
                        cfg.Blocks.Add(condJumpBlock);
                        AddDirectedEdge(currentBlock, condJumpBlock);
                        currentBlock.IsDirty = true;
                        currentBlock = condJumpBlock;
                    }
                    else
                    {
                        AddDirectedEdge(currentBlock, cfg.ExitBlock);
                    }

                    break;
                case MilOpCode.Return:
                    currentBlock.AddInstruction(instructions[i]);
                    if (!isLast)
                    {
                        var returnBlock = new Block() { Id = cfg._nextId++ };
                        cfg.Blocks.Add(returnBlock);
                        AddDirectedEdge(currentBlock, cfg.ExitBlock);
                        currentBlock = returnBlock;
                    }
                    else
                    {
                        AddDirectedEdge(currentBlock, cfg.ExitBlock);
                    }

                    break;
                case MilOpCode.Call:
                    currentBlock.AddInstruction(instructions[i]);
                    if (!isLast)
                    {
                        var callBlock = new Block() { Id = cfg._nextId++ };
                        cfg.Blocks.Add(callBlock);
                        AddDirectedEdge(currentBlock, callBlock);
                        currentBlock = callBlock;
                    }
                    else
                    {
                        AddDirectedEdge(currentBlock, cfg.ExitBlock);
                    }

                    break;
                default:
                    currentBlock.AddInstruction(instructions[i]);
                    if (isLast)
                        decompiler.Warn("Method should end with control flow instruction", "Control Flow Graph");
                    break;
            }
        }

        for (var i = 0; i < cfg.Blocks.Count; i++)
        {
            var block = cfg.Blocks[i];

            if (block.IsDirty)
                cfg.FixBlock(block, decompiler);
        }

        return cfg;
    }

    /// <summary>
    /// Initially blocks are split by calls, this merges those blocks.
    /// </summary>
    public void MergeCallBlocks()
    {
        for (var i = 0; i < Blocks.Count - 1; i++)
        {
            var block = Blocks[i];
            if (!block.IsCall) continue;
            var nextBlock = block.Successors[0];

            // make sure that the next block only has one predecessor (this)
            if (nextBlock.Predecessors.Count != 1 || nextBlock.Predecessors[0] != block) continue;

            // merge blocks
            block.Instructions.AddRange(nextBlock.Instructions);
            block.Successors = nextBlock.Successors;

            // update the predecessors of the new successors
            foreach (var successor in nextBlock.Successors)
            {
                for (var j = 0; j < successor.Predecessors.Count; j++)
                {
                    if (successor.Predecessors[j] == nextBlock)
                        successor.Predecessors[j] = block;
                }
            }

            // remove the merged block
            Blocks.RemoveAt(i + 1);
            i--;
        }
    }

    private void FixBlock(Block block, Decompiler decompiler)
    {
        if (block.IsFallThrough)
            return;

        var branch = block.Instructions.Last();
        var target = branch.Operands[0] as MilInstruction;
        var targetBlock = GetBlockByInstruction(target);

        if (targetBlock == null)
        {
            decompiler.Warn($"Unable to find branch target, maybe tail call? ({branch})", "Control Flow Graph");
            return;
        }

        var index = targetBlock.Instructions.FindIndex(i => i == target);
        var targetBlock2 = SplitAndCreate(targetBlock, index);
        AddDirectedEdge(block, targetBlock2);
        block.IsDirty = false;
    }

    /// <summary>
    /// Tries to find a block that contains an instruction.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    /// <returns>The block that contains the instruction.</returns>
    public Block? GetBlockByInstruction(MilInstruction? instruction)
    {
        if (instruction == null) return null;

        foreach (var block in Blocks)
        {
            if (block.Instructions.Any(i => i == instruction))
                return block;
        }

        return null;
    }

    private Block SplitAndCreate(Block target, int index)
    {
        if (index < 0 || index >= target.Instructions.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        // Don't need to split
        if (index == 0)
            return target;

        // split it
        var newBlock = new Block() { Id = _nextId++ };

        // take the instructions for the second part
        var instructions = target.Instructions.GetRange(index, target.Instructions.Count - index);
        target.Instructions.RemoveRange(index, target.Instructions.Count - index);

        // add those to the newNode
        newBlock.Instructions.AddRange(instructions);

        // transfer successors
        newBlock.Successors = target.Successors;
        if (target.IsDirty)
            newBlock.IsDirty = true;
        target.IsDirty = false;
        target.Successors = [];

        // correct the predecessors for all the successors
        foreach (var successor in newBlock.Successors)
        {
            for (var i = 0; i < successor.Predecessors.Count; i++)
            {
                if (successor.Predecessors[i].Id == target.Id)
                    successor.Predecessors[i] = newBlock;
            }
        }

        // add new block and connect it
        Blocks.Add(newBlock);
        AddDirectedEdge(target, newBlock);

        return newBlock;
    }

    private static void AddDirectedEdge(Block from, Block to)
    {
        from.Successors.Add(to);
        to.Predecessors.Add(from);
    }
}
