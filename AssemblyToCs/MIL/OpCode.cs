namespace AssemblyToCs.MIL;

/// <summary>
/// All medium level IL opcodes.
/// </summary>
public enum OpCode
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
    /// Jump [location]
    /// </summary>
    Jump,

    /// <summary>
    /// ConditionalJump [location], [condition]
    /// </summary>
    ConditionalJump,

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
    Xor
}
