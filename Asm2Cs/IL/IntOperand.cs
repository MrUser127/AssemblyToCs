namespace Asm2Cs.IL;

/// <summary>
/// Integer operand.
/// </summary>
public class IntOperand : IILOperand
{
    public OperandType OperandType => OperandType.Int;

    /// <summary>
    /// The value.
    /// </summary>
    public int Value;

    /// <summary>
    /// Creates a new int operand.
    /// </summary>
    /// <param name="value">The value.</param>
    public IntOperand(int value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("X");
}
