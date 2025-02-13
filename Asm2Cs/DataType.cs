using System.Text;

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
    /// Fields.
    /// </summary>
    public List<Field> Fields;

    /// <summary>
    /// Core type. int, string, etc.
    /// </summary>
    public CoreDataType CoreType;

    /// <summary>
    /// Creates a new data type.
    /// </summary>
    /// <param name="name">Name of the type.</param>
    /// <param name="fields">Fields.</param>
    /// <param name="coreType">Core type. int, string, etc.</param>
    public DataType(string name, List<Field>? fields, CoreDataType coreType)
    {
        Name = name;
        Fields = fields ?? new List<Field>();
        CoreType = coreType;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Name);
        sb.AppendLine("Fields:");

        foreach (var field in Fields)
        {
            sb.AppendLine(field.ToString());
        }

        return sb.ToString();
    }
}
