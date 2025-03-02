namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL local variable.
/// </summary>
public class MilLocalVariable(string name, int version) : IEquatable<MilLocalVariable>
{
    /// <summary>
    /// Name of the variable.
    /// </summary>
    public string Name = name;

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
        return new MilLocalVariable(Name, version);
    }

    public override string ToString() => Version == -1 ? Name : $"{Name}_{Version}";

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
