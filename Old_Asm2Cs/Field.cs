namespace Old_Asm2Cs;

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
    /// If this is a field in enum, this is its value.
    /// </summary>
    public int? EnumValue;

    /// <summary>
    /// Creates a new field.
    /// </summary>
    /// <param name="name">Name of the field.</param>
    /// <param name="type">Type of the field.</param>
    /// <param name="enumValue">If this is a field in enum, this is its value.</param>
    public Field(string name, DataType type, int? enumValue = null)
    {
        Name = name;
        Type = type;
        EnumValue = enumValue;
    }

    public override string ToString() => EnumValue == null ? $"{Type} {Name};" : $"{Type} {Name} = {EnumValue};";
}
