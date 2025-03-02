using AsmResolver.DotNet.Signatures;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Removes unreachable blocks from the control flow graph.
/// </summary>
public class RemoveUnreachableBlocks : ITransform
{
    public void Apply(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        if (method.FlowGraph == null)
            throw new NullReferenceException("Control flow graph has not been built!");

        decompiler.Info("Removing unreachable blocks...", "Remove Unreachable Blocks");
        var cfg = method.FlowGraph;

        if (cfg.Blocks.Count == 0)
            return;

        // get blocks reachable from entry
        var reachable = new List<Block>();
        var visited = new List<Block>();
        visited.Add(cfg.EntryBlock);
        reachable.Add(cfg.EntryBlock);

        var total = 0;
        while (total < reachable.Count)
        {
            var block = reachable[total];
            total++;

            foreach (var successor in block.Successors)
            {
                if (visited.Contains(successor))
                    continue;
                visited.Add(successor);
                reachable.Add(successor);
            }
        }

        // get unreachable blocks
        var unreachable = cfg.Blocks.Where(block => !visited.Remove(block)).ToList();
        var instructionCount = 0;
        var blockCount = 0;

        // remove those
        foreach (var block in unreachable)
        {
            // dont remove entry or exit
            if (block == cfg.EntryBlock || block == cfg.ExitBlock)
                continue;

            foreach (var instruction in block.Instructions)
            {
                method.Instructions.Remove(instruction);
                instructionCount++;
            }

            block.Successors.Clear();
            cfg.Blocks.Remove(block);
            blockCount++;
        }

        if (blockCount > 0)
            decompiler.Info($"Removed {blockCount} unreachable blocks and {instructionCount} instructions",
                "Remove Unreachable Blocks");
    }
}
