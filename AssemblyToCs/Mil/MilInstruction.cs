using AsmResolver.DotNet;

namespace AssemblyToCs.Mil;

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
    /// Is this instruction the start of a block?
    /// </summary>
    public bool IsBlockStart = false;

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

    public override string ToString() => $"{Offset:X4} {OpCode} {string.Join(", ", Operands.Select(FormatOperand))}";

    private static string FormatOperand((object, MilOperand) operand)
    {
        // i tried to do this with switch and ternary operators but it was way too messy

        var type = operand.Item2;
        var value = operand.Item1;

        switch (type)
        {
            case MilOperand.None:
                return "none";
            case MilOperand.Int:
            {
                var intOp = (int)operand.Item1;
                if (intOp >= 0)
                    return $"{intOp:X2}";
                return $"-{-intOp:X2}";
            }
            case MilOperand.Float:
                return value.ToString()!;
            case MilOperand.String:
                return $"\"{value}\"";
            case MilOperand.Method:
                return ((MethodDefinition)value).Name!;
            case MilOperand.Branch:
                return "@" + ((MilInstruction)value).Offset.ToString("X4");
            case MilOperand.Register:
                return $"reg{value}";
            case MilOperand.Memory:
            {
                var memory = ((int, int))value;
                if (memory.Item1 == -1)
                    return $"[{memory.Item2:X2}]";
                if (memory.Item2 == -1)
                    return $"[reg{memory.Item1}]";
                return $"[reg{memory.Item1}+{memory.Item2:X2}]";
            }
            case MilOperand.Stack:
                return $"stack[{(int)value:X2}]";
            default:
                return value.ToString()!;
        }
    }
}
