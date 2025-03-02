using AsmResolver.DotNet.Signatures;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Builds dominance info for a method.
/// </summary>
public class BuildDominance : ITransform
{
    public void Apply(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        decompiler.Info("Building dominance info...", "Build Dominance");
        method.Dominance = Dominance.Build(method);
    }
}
