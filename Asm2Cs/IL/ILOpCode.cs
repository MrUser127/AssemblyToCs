namespace Asm2Cs;

/// <summary>
/// Intermediate language opcodes.
/// </summary>
public enum ILOpCode
{
    /// <summary>
    /// There can be string as the first operand, it will be printed as assembly.
    /// </summary>
    Unknown,
    Move,
    Load,
    Store,
    Return,
    Jump,
    ConditionalJump,
    IndirectJump,
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
