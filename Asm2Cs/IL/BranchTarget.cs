namespace Asm2Cs;

/// <summary>
/// Branch target, instruction or block.
/// </summary>
public class BranchTarget : IILOperand
{
    public OperandType OperandType => OperandType.BranchTarget;

    /// <summary>
    /// Target instruction.
    /// </summary>
    public ILInstruction Instruction;

    /// <summary>
    /// Target block, can be null if control flow graph hasn't been built yet.
    /// </summary>
    public Block? Block;

    /// <summary>
    /// Creates a new branch target.
    /// </summary>
    /// <param name="instruction">Target instruction.</param>
    /// <param name="block">Target block, can be null if control flow graph hasn't been built yet.</param>
    public BranchTarget(ILInstruction instruction, Block? block = null)
    {
        Instruction = instruction;
        Block = block;
    }

    public override string ToString() => Block == null ? $"@{Instruction.Index}" : $"@{Instruction.Index}:{Block.ID}";
}
