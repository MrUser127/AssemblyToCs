namespace Asm2Cs;

public class DataTypeManager
{
    public List<DataType> AllTypes;

    public DataType IntType;
    public DataType FloatType;
    public DataType BoolType;
    public DataType StringType;

    public DataTypeManager()
    {
        IntType = new DataType("int", null, CoreDataType.Int);
        FloatType = new DataType("float", null, CoreDataType.Float);
        BoolType = new DataType("bool", null, CoreDataType.Bool);
        StringType = new DataType("string", null, CoreDataType.String);

        AllTypes = new List<DataType>()
        {
            FloatType,
            IntType,
            BoolType,
            StringType,
        };
    }

    public DataType? GetType(string name) => AllTypes.FirstOrDefault(x => x.Name == name);
}
