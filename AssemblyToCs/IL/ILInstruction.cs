namespace AssemblyToCs.IL;

/// <summary>
/// IL instruction.
/// </summary>
public class ILInstruction
{
    /// <summary>
    /// Instruction offset.
    /// </summary>
    public uint Offset;

    /// <summary>
    /// The opcode.
    /// </summary>
    public ILOpCode OpCode;

    /// <summary>
    /// Operands.
    /// </summary>
    public (object, OperandType)[] Operands;

    /// <summary>
    /// Creates a new instruction.
    /// </summary>
    /// <param name="offset">Instruction offset.</param>
    /// <param name="opCode">The opcode.</param>
    /// <param name="operands">Operands.</param>
    public ILInstruction(uint offset, ILOpCode opCode, params (object, OperandType)[] operands)
    {
        Offset = offset;
        OpCode = opCode;
        Operands = operands;
    }

    public override string ToString() => $"{Offset:X} {OpCode} {string.Join(", ", Operands.Select(FormatOperand))}";

    private static string FormatOperand((object, OperandType) operand)
    {
        return operand.Item2 switch
        {
            OperandType.Int => operand.Item1.ToString()!,
            OperandType.Float => operand.Item1.ToString()!,
            OperandType.String => $"\"{operand.Item1}\"",
            OperandType.Instruction => $"@{((ILInstruction)operand.Item1).Offset:X}",
            OperandType.InstructionResult => $"({operand.Item1})",
            OperandType.Register => $"reg{operand.Item1}",
            OperandType.StackVariable => $"stk{operand.Item1}",
            _ => operand.Item1.ToString()!
        };
    }
}
