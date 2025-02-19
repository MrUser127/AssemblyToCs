namespace Asm2Cs.IL;

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
    Int,
    Float,
    String
}
