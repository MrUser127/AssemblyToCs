using System.Text;

namespace Asm2Cs;

/// <summary>
/// All data for a function.
/// </summary>
public class Function
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
    /// Data type manager, needed for locals.
    /// </summary>
    public DataTypeManager TypeManager;

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

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Name);

        sb.AppendLine();
        sb.AppendLine("Parameters:");

        foreach (var parameter in Parameters)
        {
            sb.AppendLine(parameter.ToString());
        }

        sb.AppendLine();
        sb.AppendLine("Instructions:");

        foreach (var instruction in Instructions)
        {
            sb.AppendLine(instruction.ToString());
        }

        return sb.ToString();
    }
}
