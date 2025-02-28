using System.Text;

namespace AssemblyToCs.Mil;

/// <summary>
/// Medium level IL memory location.
/// </summary>
public struct MilMemoryLocation(int? register, int? offset) : IEquatable<MilMemoryLocation>
{
    /// <summary>
    /// Register number.
    /// </summary>
    public int? Register = register;

    /// <summary>
    /// The offset.
    /// </summary>
    public int? Offset = offset;

    public override string ToString()
    {
        if (Register == null)
        {
            if (Offset < 0)
                return $"[-{-Offset:X2}]";
            return $"[{Offset:X2}]";
        }

        if (Offset == null)
            return $"[reg{Register}]";

        var sb = new StringBuilder();
        sb.Append('[');

        if (Register != null)
            sb.Append("reg" + Register);

        if (Register != null && Offset > 0)
        {
            sb.Append('+');
            sb.Append(((int)Offset).ToString("X2"));
        }
        else
        {
            sb.Append('-');
            sb.Append((-(int)Offset).ToString("X2"));
        }

        sb.Append($"]");
        return sb.ToString();
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
        return Register == other.Register && Offset == other.Offset;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Register, Offset);
    }
}
