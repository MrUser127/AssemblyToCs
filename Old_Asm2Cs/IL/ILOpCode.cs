namespace Old_Asm2Cs.IL;

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

    /// <summary>
    /// Return [value]
    /// </summary>
    Return,
    Jump,

    /// <summary>
    /// ConditionalJump [target], [condition]
    /// </summary>
    ConditionalJump,
    IndirectJump,
    Push,
    Pop,

    /// <summary>
    /// Call [function (global variable)], [arg1], [arg2], ...
    /// </summary>
    Call,
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulus,
    Xor
}
