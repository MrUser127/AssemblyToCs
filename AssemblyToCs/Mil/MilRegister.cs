namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL register.
/// </summary>
public readonly struct MilRegister(int number) : IEquatable<MilRegister>
{
    /// <summary>
    /// Register number.
    /// </summary>
    public readonly int Number = number;

    public override string ToString() => $"reg{Number}";

    public static bool operator ==(MilRegister left, MilRegister right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MilRegister left, MilRegister right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MilRegister register)
            return false;
        return Equals(register);
    }

    public bool Equals(MilRegister other)
    {
        return Number == other.Number;
    }

    public override int GetHashCode()
    {
        return Number;
    }
}
