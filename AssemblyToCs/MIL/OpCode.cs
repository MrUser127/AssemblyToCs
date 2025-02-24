namespace AssemblyToCs.MIL;

/// <summary>
/// All medium level IL opcodes.
/// </summary>
public enum OpCode
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
