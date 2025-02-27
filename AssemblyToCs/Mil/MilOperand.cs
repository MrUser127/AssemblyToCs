namespace AssemblyToCs.Mil;

/// <summary>
/// All medium level IL operand types.
/// </summary>
public enum MilOperand
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
    Stack
}
