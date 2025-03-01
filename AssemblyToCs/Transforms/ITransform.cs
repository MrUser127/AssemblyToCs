namespace AssemblyToCs.Transforms;

/// <summary>
/// Transform that is applied to a method.
/// </summary>
public interface ITransform
{
    /// <summary>
    /// Applies the transform to a method.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="decompiler">The decompiler.</param>
    void Apply(Method method, Decompiler decompiler);
}
