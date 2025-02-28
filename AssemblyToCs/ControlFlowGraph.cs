using System.Collections.ObjectModel;
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
    public ControlFlowGraph()
    {
        EntryBlock = new Block() { Id = _nextId++ };
        ExitBlock = new Block() { Id = _nextId++ };
        Blocks = [EntryBlock, ExitBlock];
    }

    private static bool TryGetBranchTargetOffset(MilInstruction instruction, out uint offset)
    {
        offset = 0;

        try
        {
            offset = ((MilInstruction)instruction.Operands[0]!).Offset;
            return true;
        }
        catch
        {
            // ignored
        }

        return false;
    }

    /// <summary>
    /// Builds a control flow graph for a method.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="decompiler">The decompiler.</param>
    public void Build(Method method, Decompiler decompiler)
    {
        var instructions = method.Instructions;

        var currentBlock = new Block() { Id = _nextId++ };

        Blocks.Add(currentBlock);
        AddDirectedEdge(EntryBlock, currentBlock);

        for (var i = 0; i < instructions.Count; i++)
        {
            var isLast = i == instructions.Count - 1;

            switch (instructions[i].OpCode)
            {
                case MilOpCode.Jump:
                    currentBlock.AddInstruction(instructions[i]);
                    if (!isLast)
                    {
                        var jumpBlock = new Block() { Id = _nextId++ };
                        Blocks.Add(jumpBlock);
                        if (TryGetBranchTargetOffset(instructions[i], out uint jumpTargetIndex))
                            currentBlock.IsDirty = true;
                        else
                            AddDirectedEdge(currentBlock, ExitBlock);
                        currentBlock = jumpBlock;
                    }
                    else
                    {
                        AddDirectedEdge(currentBlock, ExitBlock);
                        currentBlock.IsDirty = true;
                    }

                    break;
                case MilOpCode.ConditionalJump:
                    currentBlock.AddInstruction(instructions[i]);
                    if (!isLast)
                    {
                        var condJumpBlock = new Block() { Id = _nextId++ };
                        Blocks.Add(condJumpBlock);
                        AddDirectedEdge(currentBlock, condJumpBlock);
                        currentBlock.IsDirty = true;
                        currentBlock = condJumpBlock;
                    }
                    else
                    {
                        AddDirectedEdge(currentBlock, ExitBlock);
                    }

                    break;
                case MilOpCode.Return:
                    currentBlock.AddInstruction(instructions[i]);
                    if (!isLast)
                    {
                        var returnBlock = new Block() { Id = _nextId++ };
                        Blocks.Add(returnBlock);
                        AddDirectedEdge(currentBlock, ExitBlock);
                        currentBlock = returnBlock;
                    }
                    else
                    {
                        AddDirectedEdge(currentBlock, ExitBlock);
                    }

                    break;
                default:
                    currentBlock.AddInstruction(instructions[i]);
                    if (isLast)
                        decompiler.Warn("Method should end with control flow instruction", "Control Flow Graph");
                    break;
            }
        }

        for (var i = 0; i < Blocks.Count; i++)
        {
            var block = Blocks[i];

            if (block.IsDirty)
                FixBlock(block, decompiler);
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

        if (index == 0)
            return target;

        var newNode = new Block() { Id = _nextId++ };

        var instructions = target.Instructions.GetRange(index, target.Instructions.Count - index);
        target.Instructions.RemoveRange(index, target.Instructions.Count - index);

        newNode.Instructions.AddRange(instructions);

        newNode.Successors = target.Successors;
        if (target.IsDirty)
            newNode.IsDirty = true;
        target.IsDirty = false;
        target.Successors = [];

        foreach (var successor in newNode.Successors)
        {
            for (var i = 0; i < successor.Predecessors.Count; i++)
            {
                if (successor.Predecessors[i].Id == target.Id)
                    successor.Predecessors[i] = newNode;
            }
        }

        Blocks.Add(newNode);
        AddDirectedEdge(target, newNode);

        return newNode;
    }

    private static void AddDirectedEdge(Block from, Block to)
    {
        from.Successors.Add(to);
        to.Predecessors.Add(from);
    }
}
