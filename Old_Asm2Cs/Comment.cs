using Old_Asm2Cs.IL;

namespace Old_Asm2Cs;

/// <summary>
/// Comment in decompiled code.
/// </summary>
public class Comment
{
    /// <summary>
    /// Comment text.
    /// </summary>
    public string Text;

    /// <summary>
    /// Where should this comment be? leave null for header.
    /// </summary>
    public ILInstruction? Instruction;

    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <param name="text">Comment text.</param>
    /// <param name="instruction">Where should this comment be? leave null for header.</param>
    public Comment(string text, ILInstruction? instruction = null)
    {
        Text = text;
        Instruction = instruction;
    }

    public override string ToString() => $"// {Text}";
}
