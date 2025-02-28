using AsmResolver.DotNet;

namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL instruction.
/// </summary>
public class MilInstruction(int index, MilOpCode opCode, params object?[] operands)
{
    /// <summary>
    /// Instruction index.
    /// </summary>
    public int Index = index;

    /// <summary>
    /// The opcode.
    /// </summary>
    public MilOpCode OpCode = opCode;

    /// <summary>
    /// Operands.
    /// </summary>
    public List<object?> Operands = operands.ToList();

    /// <summary>
    /// Is this instruction the start of a block?
    /// </summary>
    public bool IsBlockStart = false;

    /// <summary>
    /// Is the instruction fall through?
    /// </summary>
    public bool IsFallThrough
    {
        get
        {
            switch (OpCode)
            {
                case MilOpCode.Return:
                case MilOpCode.Jump:
                case MilOpCode.JumpTrue:
                case MilOpCode.JumpFalse:
                case MilOpCode.JumpEqual:
                case MilOpCode.JumpGreater:
                case MilOpCode.JumpLess:
                    return false;
                default:
                    return true;
            }
        }
    }

    public override string ToString() => $"{Index} {OpCode} {string.Join(", ", Operands.Select(FormatOperand))}";

    private static string FormatOperand(object? operand)
    {
        return operand switch
        {
            int num => num < 0 ? $"-{-num:X2}" : $"{num:X2}",
            string text => $"\"{text}\"",
            MethodDefinition method => method.Name!,
            MilInstruction instruction => $"@{instruction.Index}",
            null => "null",
            _ => operand.ToString()!
        };
    }
}
