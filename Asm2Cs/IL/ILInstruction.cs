namespace Asm2Cs;

/// <summary>
/// Single intermediate language instruction. Can be used as an operand.
/// </summary>
public class ILInstruction : IILOperand
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
    public IILOperand[] Operands;

    public OperandType OperandType => OperandType.InstructionResult;

    /// <summary>
    /// Creates a new instruction.
    /// </summary>
    /// <param name="index">Index of the instruction.</param>
    /// <param name="opCode">Opcode of the instruction.</param>
    /// <param name="operands">Operands of the instruction.</param>
    public ILInstruction(int index, ILOpCode opCode, params IILOperand[] operands)
    {
        Index = index;
        OpCode = opCode;
        Operands = operands;
    }

    /// <summary>
    /// Returns the instruction in the format of <c>index. opcode operand1, operand2, etc.</c>
    /// </summary>
    /// <returns>The instruction as string.</returns>
    public override string ToString()
    {
        return $"({Index}. {OpCode} {string.Join(", ", (ReadOnlySpan<object?>)Operands)})";
    }
}
