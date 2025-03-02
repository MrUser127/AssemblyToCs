using AsmResolver.DotNet;

namespace AssemblyToCs.Mil;

/// <summary>
/// Helper class for creating medium-level IL instructions. 
/// (When creating instructions, always use this; don't create instructions manually.)
/// </summary>
public class MilBuilder
{
    public List<MilInstruction> Instructions = new List<MilInstruction>();

    /// <summary>
    /// If the first operand in a branch is an int, this replaces it with the instruction at that index.
    /// </summary>
    public void FixBranches()
    {
        foreach (var instruction in Instructions)
        {
            if (!instruction.IsFallThrough && instruction.Operands[0] is int)
            {
                var index = (int)instruction.Operands[0]!;
                instruction.Operands[0] = Instructions.FirstOrDefault(i => i.Index == index);
            }
        }
    }

    /// <summary>
    /// Sets a new index for all instructions.
    /// </summary>
    public void FixIndexes()
    {
        for (var i = 0; i < Instructions.Count; i++)
            Instructions[i].Index = i;
    }

    /// <summary>
    /// Unknown (optional text, it will be included in an exception).
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="text">The optional message.</param>
    public void Unknown(int index, string? text = null) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Unknown, text));

    /// <summary>
    /// No operation.
    /// </summary>
    public void Nop(int index) => Instructions.Add(new MilInstruction(index, MilOpCode.Nop));

    /// <summary>
    /// Moves a value from <paramref name="src"/> to <paramref name="dest"/>.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="dest">The destination.</param>
    /// <param name="src">The source.</param>
    public void Move(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Move, dest, src));

    /// <summary>
    /// Pushes a value onto the stack.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="src">The value to push.</param>
    /// <param name="size">Size of the value.</param>
    public void Push(int index, object src, int size)
    {
        Instructions.Add(new MilInstruction(index, MilOpCode.ShiftStack, -size));
        Instructions.Add(new MilInstruction(index, MilOpCode.Move, new MilStackOffset(0), src));
    }

    /// <summary>
    /// Pops a value from the stack and stores it in <paramref name="dest"/>.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="dest">The destination for the popped value.</param>
    /// <param name="size">Size of the value.</param>
    public void Pop(int index, object dest, int size)
    {
        Instructions.Add(new MilInstruction(index, MilOpCode.Move, dest, new MilStackOffset(0)));
        Instructions.Add(new MilInstruction(index, MilOpCode.ShiftStack, size));
    }

    /// <summary>
    /// Adjusts the stack pointer by <paramref name="offset"/>.
    /// </summary>
    public void ShiftStack(int index, int offset) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.ShiftStack, offset));

    /// <summary>
    /// Calls a method with args and stores the return value.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="ret">The return value (null if void).</param>
    /// <param name="method">The method to call.</param>
    /// <param name="args">The args for the method.</param>
    public void Call(int index, object? ret, MethodDefinition method, params object[] args)
    {
        // i can't put args directly into constructor because then it would be object[]
        List<object?> operands = [ret, method];
        operands.AddRange(args);
        Instructions.Add(new MilInstruction(index, MilOpCode.Call, operands.ToArray()));
    }

    /// <summary>
    /// Returns from a method with an optional return value.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="src">The return value (null if void).</param>
    public void Return(int index, object? src = null) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Return, src));

    /// <summary>
    /// Unconditionally jumps to <paramref name="target"/>.
    /// </summary>
    public void Jump(int index, object target) => Instructions.Add(new MilInstruction(index, MilOpCode.Jump, target));

    /// <summary>
    /// Jumps to <paramref name="target"/> if <paramref name="condition"/> is true.
    /// </summary>
    public void JumpTrue(int index, object target, object condition) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.JumpTrue, target, condition));

    /// <summary>
    /// Jumps to <paramref name="target"/> if <paramref name="condition"/> is false.
    /// </summary>
    public void JumpFalse(int index, object target, object condition) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.JumpFalse, target, condition));

    /// <summary>
    /// Jumps to <paramref name="target"/> if <paramref name="l"/> is equal to <paramref name="r"/>.
    /// </summary>
    public void JumpEqual(int index, object target, object l, object r) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.JumpEqual, target, l, r));

    /// <summary>
    /// Jumps to <paramref name="target"/> if <paramref name="l"/> is greater than <paramref name="r"/>.
    /// </summary>
    public void JumpGreater(int index, object target, object l, object r) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.JumpGreater, target, l, r));

    /// <summary>
    /// Jumps to <paramref name="target"/> if <paramref name="l"/> is less than <paramref name="r"/>.
    /// </summary>
    public void JumpLess(int index, object target, object l, object r) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.JumpLess, target, l, r));

    /// <summary>
    /// Checks if <paramref name="l"/> is equal to <paramref name="r"/> and stores the result in <paramref name="ret"/>.
    /// </summary>
    public void CheckEqual(int index, object ret, object l, object r) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.CheckEqual, ret, l, r));

    /// <summary>
    /// Checks if <paramref name="l"/> is greater than <paramref name="r"/> and stores the result in <paramref name="ret"/>.
    /// </summary>
    public void CheckGreater(int index, object ret, object l, object r) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.CheckGreater, ret, l, r));

    /// <summary>
    /// Checks if <paramref name="l"/> is less than <paramref name="r"/> and stores the result in <paramref name="ret"/>.
    /// </summary>
    public void CheckLess(int index, object ret, object l, object r) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.CheckLess, ret, l, r));

    /// <summary>
    /// Adds <paramref name="src"/> to <paramref name="dest"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Add(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Add, dest, src));

    /// <summary>
    /// Subtracts <paramref name="src"/> from <paramref name="dest"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Subtract(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Subtract, dest, src));

    /// <summary>
    /// Multiplies <paramref name="dest"/> by <paramref name="src"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Multiply(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Multiply, dest, src));

    /// <summary>
    /// Divides <paramref name="dest"/> by <paramref name="src"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Divide(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Divide, dest, src));

    /// <summary>
    /// Performs an and operation on <paramref name="dest"/> and <paramref name="src"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void And(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.And, dest, src));

    /// <summary>
    /// Performs an or operation on <paramref name="dest"/> and <paramref name="src"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Or(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Or, dest, src));

    /// <summary>
    /// Performs a xor operation on <paramref name="dest"/> and <paramref name="src"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Xor(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Xor, dest, src));

    /// <summary>
    /// Performs a not operation on <paramref name="src"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Not(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Not, dest, src));

    /// <summary>
    /// Negates <paramref name="src"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void Negate(int index, object dest, object src) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.Negate, dest, src));

    /// <summary>
    /// Shifts <paramref name="dest"/> right by <paramref name="count"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void ShiftRight(int index, object dest, int count) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.ShiftRight, dest, count));

    /// <summary>
    /// Shifts <paramref name="dest"/> left by <paramref name="count"/> and stores the result in <paramref name="dest"/>.
    /// </summary>
    public void ShiftLeft(int index, object dest, int count) =>
        Instructions.Add(new MilInstruction(index, MilOpCode.ShiftLeft, dest, count));
}
