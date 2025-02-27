namespace AssemblyToCs.Mil;

/// <summary>
/// All medium level IL opcodes.
/// </summary>
public enum MilOpCode
{
    /// <summary>
    /// Unknown [optional text]
    /// </summary>
    Unknown,

    /// <summary>
    /// Move [dest], [src]
    /// </summary>
    Move,

    /// <summary>
    /// Return [optional src]
    /// </summary>
    Return,

    /// <summary>
    /// Add [dest], [src]
    /// </summary>
    Add,

    /// <summary>
    /// Subtract [dest], [src]
    /// </summary>
    Subtract,

    /// <summary>
    /// Multiply [dest], [src]
    /// </summary>
    Multiply,

    /// <summary>
    /// Divide [dest], [src]
    /// </summary>
    Divide,

    /// <summary>
    /// Xor [dest], [src]
    /// </summary>
    Xor,

    /// <summary>
    /// Push [src]
    /// </summary>
    Push,

    /// <summary>
    /// Pop [dest]
    /// </summary>
    Pop,

    /// <summary>
    /// Call [method], [return], [arg1], [arg2]...
    /// </summary>
    Call,

    /// <summary>
    /// Jump [location]
    /// </summary>
    Jump,

    /// <summary>
    /// ConditionalJump [location], [condition]
    /// </summary>
    ConditionalJump,

    /// <summary>
    /// CheckEqual [result], [left], [right]
    /// </summary>
    CheckEqual,

    /// <summary>
    /// CheckNotEqual [result], [left], [right]
    /// </summary>
    CheckNotEqual,

    /// <summary>
    /// CheckGreater [result], [left], [right]
    /// </summary>
    CheckGreater,

    /// <summary>
    /// CheckGreaterOrEqual [result], [left], [right]
    /// </summary>
    CheckGreaterOrEqual,

    /// <summary>
    /// CheckLess [result], [left], [right]
    /// </summary>
    CheckLess,

    /// <summary>
    /// CheckLessOrEqual [result], [left], [right]
    /// </summary>
    CheckLessOrEqual,

    /// <summary>
    /// CheckSign [result], [src]
    /// </summary>
    CheckSign,

    /// <summary>
    /// CheckNotSign [result], [src]
    /// </summary>
    CheckNotSign
}
