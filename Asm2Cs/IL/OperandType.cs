namespace Asm2Cs;

/// <summary>
/// All operand types.
/// </summary>
public enum OperandType
{
    InstructionResult,
    BranchTarget,
    Register,
    StackVariable,
    GlobalVariable,
    Function,
    Integer,
    Float,
    String
}
