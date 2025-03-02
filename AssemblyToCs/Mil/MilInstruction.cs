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

    /// <summary>
    /// Does the instruction assign a value to something?
    /// </summary>
    public bool IsAssignment
    {
        get
        {
            switch (OpCode)
            {
                case MilOpCode.Unknown:
                case MilOpCode.Nop:
                case MilOpCode.Call when Operands[0] == null:
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

    /// <summary>
    /// If this assigns a value to something, this is what the value is assigned to.
    /// </summary>
    public object? Destination
    {
        get => Operands[0];
        set => Operands[0] = value;
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
