namespace Asm2Cs;

public class DataTypeManager
{
    public List<DataType> AllTypes;

    public DataType IntType;
    public DataType FloatType;
    public DataType StringType;
    public DataType BoolType;

    public DataTypeManager()
    {
        FloatType = new DataType("float");
        IntType = new DataType("int");
        BoolType = new DataType("bool");
        StringType = new DataType("string");

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
