using AsmResolver.DotNet;

namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL instruction.
/// </summary>
public class MilInstruction(uint offset, MilOpCode opCode, params object?[] operands)
{
    /// <summary>
    /// Instruction offset.
    /// </summary>
    public uint Offset = offset;

    /// <summary>
    /// The opcode.
    /// </summary>
    public MilOpCode OpCode = opCode;

    /// <summary>
    /// Operands.
    /// </summary>
    public object?[] Operands = operands;

    /// <summary>
    /// Is this instruction the start of a block?
    /// </summary>
    public bool IsBlockStart = false;

    /// <summary>
    /// Is the instruction fall through?
    /// </summary>
    public bool IsFallThrough =>
        OpCode != MilOpCode.Jump && OpCode != MilOpCode.ConditionalJump && OpCode != MilOpCode.Return;

    public override string ToString() => $"{Offset:X2} {OpCode} {string.Join(", ", Operands.Select(FormatOperand))}";

    private static string FormatOperand(object? operand)
    {
        return operand switch
        {
            int num => num < 0 ? $"-{-num:X2}" : $"{num:X2}",
            string text => $"\"{text}\"",
            MethodDefinition method => method.Name!,
            MilInstruction instruction => $"@{instruction.Offset:X2}",
            null => "null",
            _ => operand.ToString()!
        };
    }
}
