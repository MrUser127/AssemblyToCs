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

    public static DataType Int = new DataType("int", new List<Field>(), CoreDataType.Int);
    public static DataType Float = new DataType("float", new List<Field>(), CoreDataType.Float);
    public static DataType Bool = new DataType("bool", new List<Field>(), CoreDataType.Bool);
    public static DataType String = new DataType("string", new List<Field>(), CoreDataType.String);

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
        if (CoreType != CoreDataType.Struct && CoreType != CoreDataType.Enum)
            return Name;

        StringBuilder sb = new StringBuilder();

        sb.Append(CoreType == CoreDataType.Struct ? "struct " : "enum ");
        sb.AppendLine(Name);
        sb.AppendLine("{");

        foreach (var field in Fields)
            sb.AppendLine("\t" + field);

        sb.AppendLine("}");
        return sb.ToString();
    }
}
