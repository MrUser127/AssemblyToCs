namespace Asm2Cs;

/// <summary>
/// Intermediate language opcodes.
/// </summary>
public enum ILOpCode
{
    Unknown,
    Move,
    Load,
    Store,
    Return,
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulus
}
