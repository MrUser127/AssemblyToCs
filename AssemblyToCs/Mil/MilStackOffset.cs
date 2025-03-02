namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL stack offset.
/// </summary>
public struct MilStackOffset(int offset) : IEquatable<MilStackOffset>
{
    /// <summary>
    /// The stack offset.
    /// </summary>
    public int Offset = offset;

    public override string ToString()
    {
        if (Offset < 0)
            return $"stack[-{(-Offset):X2}]";
        return $"stack[{Offset:X2}]";
    }

    public static bool operator ==(MilStackOffset left, MilStackOffset right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MilStackOffset left, MilStackOffset right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MilStackOffset stackOffset)
            return false;
        return stackOffset.Offset == Offset;
    }

    public bool Equals(MilStackOffset other)
    {
        return Offset == other.Offset;
    }

    public override int GetHashCode()
    {
        return Offset;
    }
}
