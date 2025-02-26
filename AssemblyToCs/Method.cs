using AsmResolver.DotNet;
using AssemblyToCs.MIL;

namespace AssemblyToCs;

/// <summary>
/// A method.
/// </summary>
public class Method
{
    /// <summary>
    /// The definition.
    /// </summary>
    public MethodDefinition Definition;

    /// <summary>
    /// All instructions for the method.
    /// </summary>
    public List<MilInstruction> Instructions;

    /// <summary>
    /// Parameter locations.
    /// </summary>
    public List<(object, MilOperand)> Parameters;

    /// <summary>
    /// Creates a new method.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <param name="instructions">All instructions for the method.</param>
    /// <param name="parameters">Parameter locations.</param>
    public Method(MethodDefinition definition, List<MilInstruction> instructions,
        List<(object, MilOperand)> parameters)
    {
        Definition = definition;
        Instructions = instructions;
        Parameters = parameters;
    }

    public override string ToString() => Definition.ToString();
}
