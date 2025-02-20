namespace Asm2Cs.IL;

/// <summary>
/// Local variable.
/// </summary>
public class LocalVariable
{
    /// <summary>
    /// Name of the variable.
    /// </summary>
    public string Name;

    /// <summary>
    /// Location of the variable.
    /// </summary>
    public (object, OperandType) Location;

    /// <summary>
    /// Type of the variable.
    /// </summary>
    public DataType Type;

    /// <summary>
    /// Creates a new local variable.
    /// </summary>
    /// <param name="name">Name of the variable.</param>
    /// <param name="location">Location of the variable.</param>
    /// <param name="locationType">Type of the variable location.</param>
    /// <param name="type">Type of the variable.</param>
    public LocalVariable(string name, (object, OperandType) location, DataType type)
    {
        Name = name;
        Location = location;
        Type = type;
    }

    public override string ToString() =>
        $"{Type.Name} {Name} /*{ILInstruction.FormatOperand(Location.Item1, Location.Item2)}*/";
}
