namespace Asm2Cs;

/// <summary>
/// All operand types.
/// </summary>
public enum OperandType
{
    InstructionResult,
    Register,
    StackVariable,
    GlobalVariable,
    Integer,
    Float,
    String
}
