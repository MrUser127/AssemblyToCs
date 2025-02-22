namespace Old_Asm2Cs.Transforms;

/// <summary>
/// Transform applied to a function.
/// </summary>
public interface ITransform
{
    /// <summary>
    /// Applies the transform to a function.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="decompiler">Decompiler instance.</param>
    void Run(Function function, Decompiler decompiler);
}
