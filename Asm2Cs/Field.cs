namespace Asm2Cs;

/// <summary>
/// Field in data type.
/// </summary>
public class Field
{
    /// <summary>
    /// Name of the field.
    /// </summary>
    public string Name;

    /// <summary>
    /// Type of the field.
    /// </summary>
    public DataType Type;

    /// <summary>
    /// Creates a new field.
    /// </summary>
    /// <param name="name">Name of the field.</param>
    /// <param name="type">Type of the field.</param>
    public Field(string name, DataType type)
    {
        Name = name;
        Type = type;
    }

    public override string ToString() => $"{Type} {Name};";
}
