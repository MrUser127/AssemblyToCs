namespace AssemblyToCs;

/// <summary>
/// Represents dominance information for a method. (this is taken and edited from github.com/SamboyCoding/Cpp2IL/blob/development-gompo-ast/Cpp2IL.Core/Graphs/DominatorInfo.cs)
/// </summary>
public class Dominance
{
    /// <summary>
    /// The dominance tree, mapping each block to its children in the tree.
    /// </summary>
    public Dictionary<Block, List<Block>> DominanceTree = new();

    /// <summary>
    /// The dominance frontier for each block.
    /// </summary>
    public Dictionary<Block, HashSet<Block>> DominanceFrontier = new();

    /// <summary>
    /// The immediate dominators of each block.
    /// </summary>
    public Dictionary<Block, Block?> ImmediateDominators = new();

    /// <summary>
    /// The immediate post-dominators of each block.
    /// </summary>
    public Dictionary<Block, Block?> ImmediatePostDominators = new();

    /// <summary>
    /// The post-dominators of each block.
    /// </summary>
    public Dictionary<Block, HashSet<Block>> PostDominators = new();

    /// <summary>
    /// The dominators of each block.
    /// </summary>
    public Dictionary<Block, HashSet<Block>> Dominators = new();

    /// <summary>
    /// Builds dominance info for a method.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <returns>Dominance info for the method.</returns>
    public static Dominance Build(Method method)
    {
        var cfg = method.FlowGraph!;

        var dominance = new Dominance();
        dominance.CalculateDominators(cfg);
        dominance.CalculatePostDominators(cfg);
        dominance.CalculateImmediateDominators(cfg);
        dominance.CalculateImmediatePostDominators(cfg);
        dominance.CalculateDominanceFrontiers(cfg);
        dominance.BuildDominanceTree(cfg);
        return dominance;
    }

    /// <summary>
    /// Checks if block <paramref name="a"/> dominates block <paramref name="b"/>.
    /// </summary>
    /// <param name="a">The potential dominator block.</param>
    /// <param name="b">The block to check dominance for.</param>
    /// <returns>True if <paramref name="a"/> dominates <paramref name="b"/>.</returns>
    public bool Dominates(Block a, Block b)
    {
        if (a == b)
            return true;
        if (Dominators.ContainsKey(b) && Dominators.ContainsKey(a))
            return Dominators[b].Contains(a);
        return false;
    }

    private void BuildDominanceTree(ControlFlowGraph cfg)
    {
        foreach (var block in ImmediateDominators.Keys)
        {
            var immediateDominator = ImmediateDominators[block];

            if (immediateDominator != null)
            {
                if (!DominanceTree.ContainsKey(immediateDominator))
                    DominanceTree[immediateDominator] = new List<Block>();

                DominanceTree[immediateDominator].Add(block);
            }
        }
    }

    private void CalculateDominators(ControlFlowGraph cfg)
    {
        Dominators.Clear();

        foreach (var block in cfg.Blocks)
        {
            if (block.Predecessors.Count == 0)
                Dominators[block] = [block];
            else
                Dominators[block] = new HashSet<Block>(cfg.Blocks);
        }

        var changed = true;

        while (changed)
        {
            changed = false;

            foreach (var block in cfg.Blocks)
            {
                if (block.Predecessors.Count == 0)
                    continue;

                var tempDoms = block.Predecessors.Count == 0
                    ? new HashSet<Block>()
                    : new HashSet<Block>(Dominators[block.Predecessors[0]]);

                for (var i = 1; i < block.Predecessors.Count; i++)
                    tempDoms.IntersectWith(Dominators[block.Predecessors[i]]);

                tempDoms.Add(block);

                if (!tempDoms.SetEquals(Dominators[block]))
                {
                    Dominators[block] = tempDoms;
                    changed = true;
                }
            }
        }
    }

    private void CalculatePostDominators(ControlFlowGraph cfg)
    {
        PostDominators.Clear();

        foreach (var block in cfg.Blocks)
        {
            if (block.Successors.Count == 0)
            {
                PostDominators[block] = new();
                PostDominators[block].Add(block);
            }
            else
            {
                PostDominators[block] = new HashSet<Block>(cfg.Blocks);
            }
        }

        var changed = true;

        while (changed)
        {
            changed = false;

            foreach (var block in cfg.Blocks)
            {
                if (block.Successors.Count == 0)
                    continue;

                var tempPostDoms = block.Successors.Count == 0
                    ? new HashSet<Block>()
                    : new HashSet<Block>(PostDominators[block.Successors[0]]);

                for (var i = 1; i < block.Successors.Count; i++)
                    tempPostDoms.IntersectWith(PostDominators[block.Successors[i]]);

                tempPostDoms.Add(block);

                if (!tempPostDoms.SetEquals(PostDominators[block]))
                {
                    PostDominators[block] = tempPostDoms;
                    changed = true;
                }
            }
        }
    }

    private void CalculateDominanceFrontiers(ControlFlowGraph cfg)
    {
        DominanceFrontier.Clear();

        foreach (var block in cfg.Blocks)
            DominanceFrontier[block] = new();

        foreach (var block in cfg.Blocks)
        {
            if (block.Predecessors.Count < 2) continue;

            foreach (var predecessor in block.Predecessors)
            {
                var runner = predecessor;

                while (runner != ImmediateDominators[block] && runner != null)
                {
                    DominanceFrontier[runner].Add(block);
                    runner = ImmediateDominators[runner];
                }
            }
        }
    }

    private void CalculateImmediatePostDominators(ControlFlowGraph cfg)
    {
        foreach (var block in cfg.Blocks)
        {
            if (block.Successors.Count == 0)
            {
                ImmediatePostDominators[block] = null;
                continue;
            }

            foreach (var candidate in PostDominators[block])
            {
                if (candidate == block)
                    continue;

                if (PostDominators[block].Count == 2)
                {
                    ImmediatePostDominators[block] = candidate;
                    break;
                }

                foreach (var otherCandidate in PostDominators[block])
                {
                    if (candidate == otherCandidate || candidate == block)
                        continue;

                    if (!PostDominators[otherCandidate].Contains(candidate))
                    {
                        ImmediatePostDominators[block] = candidate;
                        break;
                    }
                }
            }
        }
    }

    private void CalculateImmediateDominators(ControlFlowGraph cfg)
    {
        foreach (var block in cfg.Blocks)
        {
            if (block.Predecessors.Count == 0)
            {
                ImmediateDominators[block] = null;
                continue;
            }

            foreach (var candidate in Dominators[block])
            {
                if (candidate == block)
                    continue;

                if (Dominators[block].Count == 2)
                {
                    ImmediateDominators[block] = candidate;
                    break;
                }

                foreach (var otherCandidate in Dominators[block])
                {
                    if (candidate == otherCandidate || candidate == block)
                        continue;

                    if (!Dominators[otherCandidate].Contains(candidate))
                    {
                        ImmediateDominators[block] = candidate;
                        break;
                    }
                }
            }
        }
    }
}
