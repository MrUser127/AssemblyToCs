namespace AssemblyToCs.IL;

/// <summary>
/// All opcodes.
/// </summary>
public enum ILOpCode
{
    Nop,
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
