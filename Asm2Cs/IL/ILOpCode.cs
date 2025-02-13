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
    Push,
    Pop,
    Call,
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulus,
    Xor
}
