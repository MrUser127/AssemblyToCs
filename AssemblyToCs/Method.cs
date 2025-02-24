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
    public List<Instruction> Instructions;

    /// <summary>
    /// Parameter locations.
    /// </summary>
    public List<(object, OperandType)> Parameters;

    /// <summary>
    /// Creates a new method.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <param name="instructions">All instructions for the method.</param>
    /// <param name="parameters">Parameter locations.</param>
    public Method(MethodDefinition definition, List<Instruction> instructions, List<(object, OperandType)> parameters)
    {
        Definition = definition;
        Instructions = instructions;
        Parameters = parameters;
    }

    public override string ToString() => Definition.ToString();
}
