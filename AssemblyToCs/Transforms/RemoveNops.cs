using AssemblyToCs.Mil;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Removes all nop instructions from a method.
/// </summary>
public class RemoveNops : ITransform
{
    public void Apply(Method method, Decompiler decompiler)
    {
        decompiler.Info("Removing nops...", "Remove Nops");

        for (var i = 0; i < method.Instructions.Count; i++)
        {
            var instruction = method.Instructions[i];

            if (instruction.OpCode == MilOpCode.Nop)
            {
                // remove from instructions and CFG
                method.Instructions.RemoveAt(i);
                method.FlowGraph!.GetBlockByInstruction(instruction)!.Instructions.Remove(instruction);
                i--;
            }
        }

        // branches are references instead of indexes so this should be fine
        for (var i = 0; i < method.Instructions.Count; i++)
            method.Instructions[i].Index = i;
    }
}
