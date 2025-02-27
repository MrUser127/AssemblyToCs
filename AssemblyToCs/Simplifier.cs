﻿using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// Tries to simplify methods.
/// </summary>
public static class Simplifier
{
    /// <summary>
    /// Applies all simplifications to a method.
    /// </summary>
    /// <param name="method">What method?</param>
    /// <param name="decompiler">The decompiler.</param>
    public static void Simplify(Method method, Decompiler decompiler)
    {
        ReplaceXorWithMove(method, decompiler);
    }

    /// <summary>
    /// Applies all simplifications to a method.
    /// </summary>
    /// <param name="method">What method?</param>
    /// <param name="decompiler">The decompiler.</param>
    public static void SimplifyControlFlow(Method method, Decompiler decompiler)
    {
        RemoveUnreachableBlocks(method, decompiler);
    }

    private static void ReplaceXorWithMove(Method method, Decompiler decompiler)
    {
        var count = 0;

        foreach (var instruction in method.Instructions)
        {
            // xor reg, reg -> move reg, 0
            if (instruction.OpCode == MilOpCode.Xor && instruction.Operands[0]!.Equals(instruction.Operands[1]))
            {
                instruction.OpCode = MilOpCode.Move;
                instruction.Operands[1] = 0;
                count++;
            }
        }

        if (count > 0)
            decompiler.Info($"{count} xor reg, reg instructions replaced with move reg, 0", "Simplifier");
    }

    private static void RemoveUnreachableBlocks(Method method, Decompiler decompiler)
    {
        var cfg = method.FlowGraph!;

        if (cfg.Blocks.Count == 0)
            return;

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

        var unreachable = cfg.Blocks.Where(block => !visited.Remove(block)).ToList();
        var instructionCount = 0;
        var blockCount = 0;

        foreach (var block in unreachable)
        {
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
                "Simplifier");
    }
}
