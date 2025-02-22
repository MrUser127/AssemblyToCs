namespace Old_Asm2Cs.IL;

/// <summary>
/// Global variable reference.
/// </summary>
public class GlobalVariableOperand
{
    /// <summary>
    /// Name of the variable.
    /// </summary>
    public string Name;

    /// <summary>
    /// Creates a new global variable reference.
    /// </summary>
    /// <param name="name">Name of the variable.</param>
    public GlobalVariableOperand(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
