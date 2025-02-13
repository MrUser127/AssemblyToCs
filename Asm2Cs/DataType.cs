namespace Asm2Cs;

/// <summary>
/// Data type.
/// </summary>
public class DataType
{
    /// <summary>
    /// Name of the type.
    /// </summary>
    public string Name;

    /// <summary>
    /// Creates a new data type.
    /// </summary>
    /// <param name="name">Name of the type.</param>
    public DataType(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
