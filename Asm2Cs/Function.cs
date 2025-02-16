using System.Text;

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
    /// Return value.
    /// </summary>
    public LocalVariable ReturnValue;

    /// <summary>
    /// Comments.
    /// </summary>
    public List<Comment> Comments = new List<Comment>();

    /// <summary>
    /// Data type manager, needed for locals.
    /// </summary>
    public DataTypeManager TypeManager;

    public OperandType OperandType => OperandType.Function;

    /// <summary>
    /// The control flow graph. Null if not built yet.
    /// </summary>
    public ControlFlowGraph? ControlFlowGraph;

    /// <summary>
    /// Creates a new function.
    /// </summary>
    /// <param name="name">Name of the function.</param>
    /// <param name="instructions">Instructions that make up the function.</param>
    /// <param name="parameters">Parameters.</param>
    /// <param name="typeManager">Data type manager, needed for locals.</param>
    /// <param name="returnValue">Return value.</param>
    public Function(string name, List<ILInstruction> instructions, List<LocalVariable> parameters,
        LocalVariable returnValue, DataTypeManager typeManager)
    {
        Name = name;
        Instructions = instructions;
        Parameters = parameters;
        Locals = parameters;
        TypeManager = typeManager;
        ReturnValue = returnValue;
    }

    /// <summary>
    /// Fully analyzes the function.
    /// </summary>
    public void Analyze()
    {
        BuildControlFlowGraph();
    }

    /// <summary>
    /// Builds control flow graph for the function.
    /// </summary>
    public void BuildControlFlowGraph() => ControlFlowGraph = new ControlFlowGraph(this);

    /// <summary>
    /// Adds a comment to the function.
    /// </summary>
    /// <param name="text">Comment text.</param>
    /// <param name="type">Type of the comment.</param>
    /// <param name="instruction">Where should this comment be? leave null for header.</param>
    public void AddComment(string text, Comment.CommentType type, ILInstruction? instruction = null)
    {
        Comments.Add(new Comment(text, type, instruction));
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var comment in Comments.Where(comment => comment.Instruction == null))
            sb.AppendLine(comment.ToString());

        sb.Append($"{ReturnValue} {Name}({string.Join(", ", Parameters)})");
        return sb.ToString();
    }
}
