namespace Asm2Cs.IL;

/// <summary>
/// Register operand.
/// </summary>
public class RegisterOperand : IILOperand
{
    public OperandType OperandType => OperandType.Register;

    /// <summary>
    /// The register number.
    /// </summary>
    public int Register;

    /// <summary>
    /// Creates a new register operand.
    /// </summary>
    /// <param name="register">The register number.</param>
    public RegisterOperand(int register)
    {
        Register = register;
    }

    public override string ToString() => $"r{Register}";
}
