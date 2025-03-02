using AsmResolver.DotNet.Signatures;

namespace AssemblyToCs.Transforms;

/// <summary>
/// Merges call blocks in the control flow graph.
/// </summary>
public class MergeCallBlocks : ITransform
{
    public void Apply(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        if (method.FlowGraph == null)
            throw new NullReferenceException("Control flow graph has not been built!");

        decompiler.Info("Merging call blocks...", "Merge Call Blocks");
        method.FlowGraph.MergeCallBlocks();
    }
}
