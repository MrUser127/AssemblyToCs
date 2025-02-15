using System.Text;

namespace Asm2Cs;

/// <summary>
/// Control flow block, first instruction is the only entry point and last is only exit.
/// </summary>
public class Block
{
    /// <summary>
    /// Unique id for the block.
    /// </summary>
    public int ID = -1;

    /// <summary>
    /// Instructions that make up the block.
    /// </summary>
    public List<ILInstruction> Instructions = [];

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
    /// <param name="instruction">The instruction.</param>
    public void AddInstruction(ILInstruction instruction) => Instructions.Add(instruction);

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var instruction in Instructions)
            sb.AppendLine(instruction.ToString());

        return sb.ToString();
    }
}
