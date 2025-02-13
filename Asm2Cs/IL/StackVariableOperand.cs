namespace Asm2Cs;

/// <summary>
/// Stack variable reference.
/// </summary>
public class StackVariableOperand : IILOperand
{
    public OperandType OperandType => OperandType.StackVariable;

    /// <summary>
    /// The stack offset.
    /// </summary>
    public int Offset;

    /// <summary>
    /// Creates a new stack variable reference.
    /// </summary>
    /// <param name="offset">The stack offset.</param>
    public StackVariableOperand(int offset)
    {
        Offset = offset;
    }

    public override string ToString() => "stack:" + Offset.ToString("X");
}
