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

            if (instruction.Operands[0].Item2 != OperandType.Register)
                continue;
            if (instruction.Operands[1].Item2 != OperandType.Register)
                continue;

            if (instruction.Operands[0] != instruction.Operands[1])
                continue;

            instruction.OpCode = ILOpCode.Move;
            instruction.Operands[1] = (0, OperandType.Int);
            instructionsTransformed++;
        }

        decompiler.LogInfo($"{instructionsTransformed} instructions replaced with move", "xor to move zero transform");
    }
}
