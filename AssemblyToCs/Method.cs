using AsmResolver.DotNet;
using AssemblyToCs.IL;

namespace AssemblyToCs;

/// <summary>
/// IL Method.
/// </summary>
public class Method
{
    /// <summary>
    /// .NET method definition.
    /// </summary>
    public MethodDefinition Definition;

    /// <summary>
    /// All instructions.
    /// </summary>
    public List<ILInstruction> Instructions;

    /// <summary>
    /// Creates a new method.
    /// </summary>
    /// <param name="definition">.NET method definition.</param>
    /// <param name="instructions">All instructions.</param>
    public Method(MethodDefinition definition, List<ILInstruction> instructions)
    {
        Definition = definition;
        Instructions = instructions;
    }

    public override string ToString() => Definition.ToString();
}
