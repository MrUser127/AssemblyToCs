using AssemblyToCs.MIL;

namespace AssemblyToCs;

/// <summary>
/// Tries to simplify methods.
/// </summary>
public static class Simplification
{
    /// <summary>
    /// Applies all simplifications to a method.
    /// </summary>
    /// <param name="method">What method?</param>
    /// <param name="decompiler">The decompiler.</param>
    public static void Apply(Method method, Decompiler decompiler)
    {
        ApplyXorToMove(method, decompiler);
    }

    private static void ApplyXorToMove(Method method, Decompiler decompiler)
    {
        var count = 0;

        foreach (var instruction in method.Instructions)
        {
            if (instruction.OpCode != OpCode.Xor)
                continue;
            if (!instruction.Operands[0].Equals(instruction.Operands[1]))
                continue;

            instruction.OpCode = OpCode.Move;
            instruction.Operands[1] = (0, OperandType.Int);
            count++;
        }

        if (count > 0)
            decompiler.Info($"{count} xor reg, reg instructions replaced with move reg, 0", "Simplification");
    }
}
