using AsmResolver.DotNet;
using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// A method.
/// </summary>
public class Method(
    MethodDefinition definition,
    List<MilInstruction> instructions,
    List<object> parameters)
{
    /// <summary>
    /// The definition.
    /// </summary>
    public readonly MethodDefinition Definition = definition;

    /// <summary>
    /// All instructions for the method.
    /// </summary>
    public List<MilInstruction> Instructions = instructions;

    /// <summary>
    /// The control flow graph, null if not built yet.
    /// </summary>
    public ControlFlowGraph? FlowGraph;

    /// <summary>
    /// Dominance info, null if not built yet.
    /// </summary>
    public Dominance? Dominance;

    /// <summary>
    /// Parameter locations.
    /// </summary>
    public List<object> Parameters = parameters;

    public override string ToString() => Definition.ToString();
}
