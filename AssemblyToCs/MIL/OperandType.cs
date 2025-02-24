namespace AssemblyToCs.MIL;

/// <summary>
/// All operand types.
/// </summary>
public enum OperandType
{
    None,
    Int,
    Float,
    String,

    /// <summary>
    /// AsmResolver MethodDefinition.
    /// </summary>
    Method,

    /// <summary>
    /// Instruction.
    /// </summary>
    Branch,

    /// <summary>
    /// Int. (register number)
    /// </summary>
    Register,

    /// <summary>
    /// Ulong. (address)
    /// </summary>
    Memory,

    /// <summary>
    /// Int. (stack offset)
    /// </summary>
    StackVariable
}
