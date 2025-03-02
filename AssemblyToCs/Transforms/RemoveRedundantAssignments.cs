using AsmResolver.DotNet.Signatures;
using AssemblyToCs.Mil;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Removes redundant assignments from methods (like if a register is assigned something else right before using it).
/// </summary>
public class RemoveRedundantAssignments : ITransform
{
    public void Apply(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        decompiler.Info("Removing redundant assignments...", "Remove Redundant Assignments");

        var count = 0;

        // repeat until no change
        var changed = true;
        while (changed)
        {
            changed = false;

            // for each instruction
            for (var i = 0; i < method.Instructions.Count - 1; i++)
            {
                var current = method.Instructions[i];
                var next = method.Instructions[i + 1];

                // don't make return register same as args
                if (next.OpCode == MilOpCode.Call)
                    continue;

                if (current.OpCode != MilOpCode.Move) continue;
                if (current.Operands[0] is not MilRegister moveDestRegister) continue;
                var moveSrc = current.Operands[1];

                // check if it's used before being reassigned (+2 to skip next)
                if (IsRegisterUsedBeforeReassignment(method, i + 2, moveDestRegister))
                    continue;

                // replace occurrences of the redundant register in the next instruction
                for (var j = 0; j < next.Operands.Count; j++)
                {
                    if (next.Operands[j] is MilRegister register && register == moveDestRegister)
                    {
                        next.Operands[j] = moveSrc;
                        RemoveInstruction(current, method);
                        changed = true;
                        count++;
                        break;
                    }
                }
            }
        }

        for (var i = 0; i < method.Instructions.Count; i++)
            method.Instructions[i].Index = i;

        if (count > 0)
            decompiler.Info($"{count} redundant assignments removed", "Remove Redundant Assignments");
    }

    private static bool IsRegisterUsedBeforeReassignment(Method method, int startIndex, MilRegister register)
    {
        for (int i = startIndex; i < method.Instructions.Count; i++)
        {
            var instruction = method.Instructions[i];

            // it's reassigned
            if (instruction.OpCode == MilOpCode.Move && instruction.Operands[0] is MilRegister destReg &&
                destReg == register)
                return false;

            // is it used?
            foreach (var operand in instruction.Operands)
            {
                if (operand is MilRegister usedReg && usedReg == register)
                    return true;
            }
        }

        return false;
    }

    private static void RemoveInstruction(MilInstruction instruction, Method method)
    {
        method.Instructions.Remove(instruction);

        if (method.FlowGraph != null)
        {
            var block = method.FlowGraph.GetBlockByInstruction(instruction);
            block?.Instructions.Remove(instruction);
        }
    }
}
