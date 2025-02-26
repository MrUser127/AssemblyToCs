using AsmResolver.DotNet;

namespace AssemblyToCs.MIL;

/// <summary>
/// Medium level IL instruction.
/// </summary>
public class MilInstruction
{
    /// <summary>
    /// Instruction offset.
    /// </summary>
    public uint Offset;

    /// <summary>
    /// The opcode.
    /// </summary>
    public MilOpCode OpCode;

    /// <summary>
    /// Operands.
    /// </summary>
    public (object, MilOperand)[] Operands;

    /// <summary>
    /// Creates a new instruction.
    /// </summary>
    /// <param name="offset">Instruction offset.</param>
    /// <param name="opCode">The opcode.</param>
    /// <param name="operands">Operands.</param>
    public MilInstruction(uint offset, MilOpCode opCode, params (object, MilOperand)[] operands)
    {
        Offset = offset;
        OpCode = opCode;
        Operands = operands;
    }

    /// <summary>
    /// Gets the type of number i operand.
    /// </summary>
    /// <param name="i">Which operand?</param>
    /// <returns>Operand type.</returns>
    public MilOperand GetOpType(int i) => Operands[i].Item2;

    /// <summary>
    /// Gets the number i operand.
    /// </summary>
    /// <param name="i">Which operand?</param>
    /// <returns>The operand.</returns>
    public object GetOp(int i) => Operands[i].Item1;

    public override string ToString() => $"{Offset:X} {OpCode} {string.Join(", ", Operands.Select(FormatOperand))}";

    private static string FormatOperand((object, MilOperand) operand)
    {
        return operand.Item2 switch
        {
            MilOperand.Int => operand.Item1.ToString()!,
            MilOperand.Float => operand.Item1.ToString()!,
            MilOperand.String => $"\"{operand.Item1}\"",
            MilOperand.Method => ((MethodDefinition)operand.Item1).Name!,
            MilOperand.Branch => $"@{((MilInstruction)operand.Item1).Offset:X}",
            MilOperand.Register => $"reg{operand.Item1}",
            MilOperand.Memory => $"mem:0x{operand.Item1:X}",
            MilOperand.Stack => $"stk:{operand.Item1}",
            _ => operand.Item1.ToString()!
        };
    }
}
