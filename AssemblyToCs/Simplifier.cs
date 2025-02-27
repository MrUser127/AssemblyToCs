using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// Tries to simplify methods.
/// </summary>
public static class Simplifier
{
    /// <summary>
    /// Applies all simplifications to a method.
    /// </summary>
    /// <param name="method">What method?</param>
    /// <param name="decompiler">The decompiler.</param>
    public static void Simplify(Method method, Decompiler decompiler)
    {
        ReplaceXorWithMove(method, decompiler);
    }

    private static void ReplaceXorWithMove(Method method, Decompiler decompiler)
    {
        var count = 0;

        foreach (var instruction in method.Instructions)
        {
            // xor reg, reg -> move reg, 0
            if (instruction.OpCode == MilOpCode.Xor && instruction.Operands[0].Equals(instruction.Operands[1]))
            {
                instruction.OpCode = MilOpCode.Move;
                instruction.Operands[1] = (0, MilOperand.Int);
                count++;
            }
        }

        if (count > 0)
            decompiler.Info($"{count} xor reg, reg instructions replaced with move reg, 0", "Simplifier");
    }
}
