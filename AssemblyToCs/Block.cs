using System.Diagnostics;
using System.Text;
using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// A block in control flow graph.
/// </summary>
[DebuggerDisplay("Id = {Id}")]
public class Block
{
    /// <summary>
    /// ID of the block.
    /// </summary>
    public int Id = -1;

    /// <summary>
    /// Is the block dirty?
    /// </summary>
    public bool IsDirty = false;

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
    /// Is the block fall through?
    /// </summary>
    public bool IsFallThrough => Instructions.Last().IsFallThrough;

    /// <summary>
    /// Is the block call?
    /// </summary>
    public bool IsCall => Instructions.Count != 0 && Instructions.Last().OpCode == MilOpCode.Call;

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
