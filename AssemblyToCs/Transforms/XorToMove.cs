using AssemblyToCs.Mil;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Replaces xor reg, reg with move reg, 0.
/// </summary>
public class XorToMove : ITransform
{
    public void Apply(Method method, Decompiler decompiler)
    {
        var count = 0;

        foreach (var instruction in method.Instructions)
        {
            // xor reg, reg
            if (instruction.OpCode == MilOpCode.Xor && instruction.Operands[0]!.Equals(instruction.Operands[1]))
            {
                // replace with move reg, 0
                instruction.OpCode = MilOpCode.Move;
                instruction.Operands[1] = 0;
                count++;
            }
        }

        if (count > 0)
            decompiler.Info($"{count} xor reg, reg instructions replaced with move reg, 0", "Xor to Move");
    }
}
