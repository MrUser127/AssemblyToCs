using Asm2Cs.IL;

namespace Asm2Cs.Transforms;

/// <summary>
/// Replaces xor reg, reg with move reg, 0
/// </summary>
public class XorToMoveZeroTransform : ITransform
{
    public void Run(Function function, Decompiler decompiler)
    {
        var instructionsTransformed = 0;

        foreach (var instruction in function.Instructions)
        {
            if (instruction.OpCode != ILOpCode.Xor)
                continue;

            if (instruction.Operands[0].OperandType != OperandType.Register)
                continue;
            if (instruction.Operands[1].OperandType != OperandType.Register)
                continue;

            if (((RegisterOperand)instruction.Operands[0]).Register !=
                ((RegisterOperand)instruction.Operands[1]).Register)
                continue;

            instruction.OpCode = ILOpCode.Move;
            instruction.Operands[1] = new IntOperand(0);
            instructionsTransformed++;
        }

        decompiler.LogInfo($"{instructionsTransformed} xor reg, reg instructions replaced with move reg, 0");
    }
}
