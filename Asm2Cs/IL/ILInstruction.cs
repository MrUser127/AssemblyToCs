namespace Asm2Cs.IL;

/// <summary>
/// Single intermediate language instruction. Can be used as an operand.
/// </summary>
public class ILInstruction
{
    /// <summary>
    /// Index of the instruction.
    /// </summary>
    public int Index;

    /// <summary>
    /// Opcode of the instruction.
    /// </summary>
    public ILOpCode OpCode;

    /// <summary>
    /// Operands of the instruction.
    /// </summary>
    public (object, OperandType)[] Operands;

    /// <summary>
    /// Is this instruction the entry point of a block?
    /// </summary>
    public bool IsBlockStart;

    public OperandType OperandType => OperandType.InstructionResult;

    /// <summary>
    /// Creates a new instruction.
    /// </summary>
    /// <param name="index">Index of the instruction.</param>
    /// <param name="opCode">Opcode of the instruction.</param>
    /// <param name="operands">Operands of the instruction.</param>
    public ILInstruction(int index, ILOpCode opCode, params (object, OperandType)[] operands)
    {
        Index = index;
        OpCode = opCode;
        Operands = operands;
    }

    /// <summary>
    /// Returns the instruction in the format of <c>index. opcode operand1, operand2, etc.</c>
    /// </summary>
    /// <returns>The instruction as string.</returns>
    public override string ToString() =>
        $"({Index}. {OpCode} {string.Join(", ", Operands.Select(operand =>
            FormatOperand(operand.Item1, operand.Item2)).ToArray())})";

    public static string FormatOperand(object operand, OperandType type)
    {
        return type switch
        {
            OperandType.Instruction => $"@{((ILInstruction)operand).Index}",
            OperandType.Register => $"r{operand}",
            OperandType.StackVariable => $"stack:{operand:X}",
            OperandType.String => $"\"{operand}\"",
            _ => operand.ToString()!
        };
    }
}
