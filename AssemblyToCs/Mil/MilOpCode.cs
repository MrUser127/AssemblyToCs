namespace AssemblyToCs.Mil;

/// <summary>
/// All medium level IL opcodes.
/// </summary>
public enum MilOpCode
{
    Unknown,
    Nop,
    Move,
    ShiftStack,
    Phi,
    Call,
    Return,
    Jump,
    JumpTrue,
    JumpFalse,
    JumpEqual,
    JumpGreater,
    JumpLess,
    CheckEqual,
    CheckGreater,
    CheckLess,
    Add,
    Subtract,
    Multiply,
    Divide,
    And,
    Or,
    Xor,
    Not,
    Negate,
    ShiftRight,
    ShiftLeft
}
