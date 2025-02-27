using System.Text;
using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// A block in control flow graph.
/// </summary>
public class Block
{
    /// <summary>
    /// ID of the block.
    /// </summary>
    public int Id = -1;

    /// <summary>
    /// Instructions that make up the block.
    /// </summary>
    public List<MilInstruction> Instructions = [];

    /// <summary>
    /// Blocks that can flow into this block.
    /// </summary>
    public List<Block> Predecessors = [];

    /// <summary>
    /// Blocks that this block can flow into.
    /// </summary>
    public List<Block> Successors = [];

    /// <summary>
    /// Adds instruction to the block.
    /// </summary>
    /// <param name="instruction">What instruction?</param>
    public void AddInstruction(MilInstruction instruction) => Instructions.Add(instruction);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        foreach (var instruction in Instructions)
            stringBuilder.AppendLine(instruction.ToString());
        return stringBuilder.ToString();
    }
}
