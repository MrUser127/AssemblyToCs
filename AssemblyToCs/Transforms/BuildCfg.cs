using AsmResolver.DotNet.Signatures;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Builds the control flow graph for a method.
/// </summary>
public class BuildCfg : ITransform
{
    public void Apply(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        decompiler.Info("Building CFG...", "Build CFG");
        var cfg = ControlFlowGraph.Build(method, decompiler);
        method.FlowGraph = cfg;
    }
}
