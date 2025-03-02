using AsmResolver.DotNet;
using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// A method.
/// </summary>
public class Method(
    MethodDefinition definition,
    List<MilInstruction> instructions,
    List<object> parameters,
    int archSize)
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
    /// Parameter locations.
    /// </summary>
    public List<object> Parameters = parameters;

    /// <summary>
    /// Local variables of the method.
    /// </summary>
    public List<MilLocalVariable> Locals = [];

    /// <summary>
    /// Architecture size. (4 for 32 bit and 8 for 64 bit)
    /// </summary>
    public int ArchSize = archSize;

    /// <summary>
    /// The control flow graph, null if not built yet.
    /// </summary>
    public ControlFlowGraph? FlowGraph;

    /// <summary>
    /// Dominance info, null if not built yet.
    /// </summary>
    public Dominance? Dominance;


    public override string ToString() => Definition.ToString();
}
