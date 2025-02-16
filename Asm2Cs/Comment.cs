namespace Asm2Cs;

/// <summary>
/// Comment in decompiled code.
/// </summary>
public class Comment
{
    /// <summary>
    /// Comment types.
    /// </summary>
    public enum CommentType
    {
        Warning,
        Error
    }

    /// <summary>
    /// Comment text.
    /// </summary>
    public string Text;

    /// <summary>
    /// Where should this comment be? leave null for header.
    /// </summary>
    public ILInstruction? Instruction;

    /// <summary>
    /// Type of the comment.
    /// </summary>
    public CommentType Type;

    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <param name="text">Comment text.</param>
    /// <param name="type">Type of the comment.</param>
    /// <param name="instruction">Where should this comment be? leave null for header.</param>
    public Comment(string text, CommentType type, ILInstruction? instruction = null)
    {
        Text = text;
        Instruction = instruction;
        Type = type;
    }

    public override string ToString() => $"// {Type}: {Text}";
}
