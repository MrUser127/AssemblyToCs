using Asm2Cs.IL;

namespace Asm2Cs;

/// <summary>
/// All data for a function, implements operand for function reference in call instruction.
/// </summary>
public class Function : IILOperand
{
    /// <summary>
    /// Name of the function.
    /// </summary>
    public string Name;

    /// <summary>
    /// Instructions that make up the function.
    /// </summary>
    public List<ILInstruction> Instructions;

    /// <summary>
    /// Parameters, these should also be in locals.
    /// </summary>
    public List<LocalVariable> Parameters;

    /// <summary>
    /// Local variables.
    /// </summary>
    public List<LocalVariable> Locals;

    /// <summary>
    /// Return type.
    /// </summary>
    public DataType ReturnType;

    /// <summary>
    /// Comments.
    /// </summary>
    public List<Comment> Comments = new List<Comment>();

    public OperandType OperandType => OperandType.Function;

    /// <summary>
    /// Creates a new function.
    /// </summary>
    /// <param name="name">Name of the function.</param>
    /// <param name="instructions">Instructions that make up the function.</param>
    /// <param name="parameters">Parameters.</param>
    /// <param name="returnType">Return type.</param>
    public Function(string name, List<ILInstruction> instructions, List<LocalVariable> parameters, DataType returnType)
    {
        Name = name;
        Instructions = instructions;
        Parameters = parameters;
        Locals = parameters;
        ReturnType = returnType;
    }

    /// <summary>
    /// Adds a comment to the function.
    /// </summary>
    /// <param name="text">Comment text.</param>
    /// <param name="instruction">Where should this comment be? leave null for header.</param>
    public void AddComment(string text, ILInstruction? instruction = null) =>
        Comments.Add(new Comment(text, instruction));

    /// <summary>
    /// Prints the function signature.
    /// </summary>
    /// <returns>The function signature.</returns>
    public string PrintSignature() => $"{ReturnType.Name} {Name}({string.Join(", ", Parameters)})";

    public override string ToString() => PrintSignature();
}
