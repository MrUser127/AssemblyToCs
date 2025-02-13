namespace Asm2Cs;

/// <summary>
/// String operand.
/// </summary>
public class StringOperand : IILOperand
{
    public OperandType OperandType => OperandType.String;

    /// <summary>
    /// The value.
    /// </summary>
    public string Value;

    /// <summary>
    /// Creates a new string operand.
    /// </summary>
    /// <param name="value">The value.</param>
    public StringOperand(string value)
    {
        Value = value;
    }

    public override string ToString() => $"\"{Value}\"";
}
