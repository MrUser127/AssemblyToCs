using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL local variable.
/// </summary>
public class MilLocalVariable(string name, int register, TypeSignature? type, int version)
    : IEquatable<MilLocalVariable>
{
    /// <summary>
    /// Name of the variable.
    /// </summary>
    public string Name = name;

    /// <summary>
    /// What register was this?
    /// </summary>
    public int Register = register;

    /// <summary>
    /// Type of the variable (null if typeprop has not been done yet).
    /// </summary>
    public TypeSignature? Type = type;

    /// <summary>
    /// SSA version of the variable.
    /// </summary>
    public int Version = version;

    /// <summary>
    /// Creates a copy of the variable with different version.
    /// </summary>
    /// <param name="version">SSA version of the variable.</param>
    /// <returns>The new variable.</returns>
    public MilLocalVariable Copy(int version)
    {
        return new MilLocalVariable(Name, Register, Type, version);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Name);
        if (Version != -1)
            sb.Append($"_{Version}");
        if (Type != null)
            sb.Append($" ({Type.Name})");
        sb.Append($" (reg{Register})");
        return sb.ToString();
    }

    public static bool operator ==(MilLocalVariable left, MilLocalVariable right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MilLocalVariable left, MilLocalVariable right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MilLocalVariable local)
            return false;
        return Equals(local);
    }

    public bool Equals(MilLocalVariable other)
    {
        return Name == other.Name && Version == other.Version;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Version);
    }
}
