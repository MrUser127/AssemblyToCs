namespace Asm2Cs.IL;

/// <summary>
/// Float operand.
/// </summary>
public class FloatOperand : IILOperand
{
    public OperandType OperandType => OperandType.Float;

    /// <summary>
    /// The value.
    /// </summary>
    public float Value;

    /// <summary>
    /// Creates a new float operand.
    /// </summary>
    /// <param name="value">The value.</param>
    public FloatOperand(float value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString();
}
