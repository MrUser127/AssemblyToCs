namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL memory location.
/// </summary>
public struct MilMemoryLocation(int offset) : IEquatable<MilMemoryLocation>
{
    /// <summary>
    /// The offset.
    /// </summary>
    public int Offset = offset;

    public override string ToString()
    {
        if (Offset < 0)
            return $"[-{-Offset:X2}]";
        return $"[{Offset:X2}]";
    }

    public static bool operator ==(MilMemoryLocation left, MilMemoryLocation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MilMemoryLocation left, MilMemoryLocation right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MilMemoryLocation memoryLocation)
            return false;
        return Equals(memoryLocation);
    }

    public bool Equals(MilMemoryLocation other)
    {
        return Offset == other.Offset;
    }

    public override int GetHashCode()
    {
        return Offset;
    }
}
