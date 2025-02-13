using System.Text;

namespace Asm2Cs;

public class Function
{
    public string Name;

    public List<ILInstruction> Instructions;

    public Function(string name, List<ILInstruction> instructions)
    {
        Name = name;
        Instructions = instructions;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Name);
        sb.AppendLine();

        foreach (var instruction in Instructions)
        {
            sb.AppendLine(instruction.ToString());
        }

        return sb.ToString();
    }
}
