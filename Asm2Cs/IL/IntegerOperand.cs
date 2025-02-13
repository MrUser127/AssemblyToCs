namespace Asm2Cs;

/// <summary>
/// Integer operand.
/// </summary>
public class IntegerOperand : IILOperand
{
    public OperandType OperandType => OperandType.Integer;

    /// <summary>
    /// The value.
    /// </summary>
    public int Value;

    /// <summary>
    /// Creates a new int operand.
    /// </summary>
    /// <param name="value">The value.</param>
    public IntegerOperand(int value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("X");
}
