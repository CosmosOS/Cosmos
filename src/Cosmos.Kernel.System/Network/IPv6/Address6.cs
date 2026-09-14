// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Cosmos.Kernel.System.Network.IPv4;

namespace Cosmos.Kernel.System.Network.IPv6;

/// <summary>
/// The scope or role an IPv6 address has by its prefix, as <see cref="Address6.AddressType"/> reads it.
/// </summary>
public enum IPv6AddressType
{
    /// <summary>
    /// An address assigned to several interfaces, delivered to the nearest one.
    /// </summary>
    Anycast,

    /// <summary>
    /// A routable address in <c>2000::/3</c>.
    /// </summary>
    GlobalUnicast,

    /// <summary>
    /// A link-local address in <c>fe80::/10</c>.
    /// </summary>
    LinkLocal,

    /// <summary>
    /// The loopback address <c>::1</c>.
    /// </summary>
    Loopback,

    /// <summary>
    /// The unspecified address <c>::</c>.
    /// </summary>
    Unspecified,

    /// <summary>
    /// A unique local address in <c>fc00::/7</c>.
    /// </summary>
    UniqueLocal,

    /// <summary>
    /// An IPv4 address carried in the low 32 bits, as in <c>::ffff:192.0.2.128</c>.
    /// </summary>
    EmbeddedIPv4,

    /// <summary>
    /// A well-known multicast address in <c>ff00::/12</c>.
    /// </summary>
    WellKnown,

    /// <summary>
    /// A transient multicast address.
    /// </summary>
    Transient,

    /// <summary>
    /// A solicited-node multicast address in <c>ff02::1:ff00:0/104</c>.
    /// </summary>
    SolicitedNode,

    /// <summary>
    /// Any address no other value describes.
    /// </summary>
    Generic,
}

/// <summary>
/// Represents an IPv6 address.
/// </summary>
public class Address6 : Address, IComparable<Address6>, IEquatable<Address6>
{
    /// <summary>
    /// The unspecified address <c>::</c>.
    /// </summary>
    public static readonly Address6 Zero = new(0, 0, 0, 0);

    /// <summary>
    /// The loopback address <c>::1</c>.
    /// </summary>
    public static readonly Address6 Loopback = new(0, 0, 0, 1);

    /// <summary>
    /// Bytes 0 to 3 packed into one number, the first byte in the most significant position.
    /// </summary>
    public uint Segment1 { get; }

    /// <summary>
    /// Bytes 4 to 7 packed into one number.
    /// </summary>
    public uint Segment2 { get; }

    /// <summary>
    /// Bytes 8 to 11 packed into one number.
    /// </summary>
    public uint Segment3 { get; }

    /// <summary>
    /// Bytes 12 to 15 packed into one number.
    /// </summary>
    public uint Segment4 { get; }

    /// <inheritdoc />
    public override bool IsZero => Equals(Zero);

    /// <inheritdoc />
    public override MaskedAddress Parts => new(Segment1, Segment2, Segment3, Segment4);

    /// <summary>
    /// Always <see langword="false"/>: IPv6 has multicast instead of broadcast.
    /// </summary>
    public override bool IsBroadcastAddress => false;

    /// <summary>
    /// Whether this is the loopback address <c>::1</c>.
    /// </summary>
    public override bool IsLoopbackAddress => Equals(Loopback);

    /// <summary>
    /// Creates an address from its four 32-bit segments.
    /// </summary>
    /// <param name="segment1">Bytes 0 to 3, most significant first.</param>
    /// <param name="segment2">Bytes 4 to 7.</param>
    /// <param name="segment3">Bytes 8 to 11.</param>
    /// <param name="segment4">Bytes 12 to 15.</param>
    public Address6(uint segment1, uint segment2, uint segment3, uint segment4)
    {
        Segment1 = segment1;
        Segment2 = segment2;
        Segment3 = segment3;
        Segment4 = segment4;
    }

    /// <summary>
    /// Creates an address from its four 32-bit segments.
    /// </summary>
    /// <param name="segments">The four segments, most significant first.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segments"/> is not exactly four values long.</exception>
    public Address6(ReadOnlySpan<uint> segments)
    {
        if (segments.Length != 4)
        {
            throw new ArgumentOutOfRangeException(nameof(segments), "Segments have to be 4 unsigned ints long");
        }

        Segment1 = segments[0];
        Segment2 = segments[1];
        Segment3 = segments[2];
        Segment4 = segments[3];
    }

    /// <summary>
    /// Creates an address from its eight 16-bit groups, the ones written between colons.
    /// </summary>
    /// <param name="segment1A">Group 1, the most significant.</param>
    /// <param name="segment1B">Group 2.</param>
    /// <param name="segment2A">Group 3.</param>
    /// <param name="segment2B">Group 4.</param>
    /// <param name="segment3A">Group 5.</param>
    /// <param name="segment3B">Group 6.</param>
    /// <param name="segment4A">Group 7.</param>
    /// <param name="segment4B">Group 8, the least significant.</param>
    public Address6(ushort segment1A, ushort segment1B, ushort segment2A, ushort segment2B,
        ushort segment3A, ushort segment3B, ushort segment4A, ushort segment4B)
    {
        Segment1 = (uint)(segment1A << 16 | segment1B);
        Segment2 = (uint)(segment2A << 16 | segment2B);
        Segment3 = (uint)(segment3A << 16 | segment3B);
        Segment4 = (uint)(segment4A << 16 | segment4B);
    }

    /// <summary>
    /// Creates an address from its eight 16-bit groups, the ones written between colons.
    /// </summary>
    /// <param name="buffer">The eight groups, most significant first.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="buffer"/> is not exactly eight values long.</exception>
    public Address6(ReadOnlySpan<ushort> buffer)
    {
        if (buffer.Length != 8)
        {
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer has to be 8 unsigned shorts long");
        }

        Segment1 = (uint)(buffer[0] << 16 | buffer[1]);
        Segment2 = (uint)(buffer[2] << 16 | buffer[3]);
        Segment3 = (uint)(buffer[4] << 16 | buffer[5]);
        Segment4 = (uint)(buffer[6] << 16 | buffer[7]);
    }

    /// <inheritdoc />
    public override ReadOnlySpan<byte> ToBytes()
    {
        Span<byte> data = new byte[16];
        SegmentToSpan(Segment1, data);
        SegmentToSpan(Segment2, data[4..7]);
        SegmentToSpan(Segment3, data[8..11]);
        SegmentToSpan(Segment4, data[12..15]);
        return data;
    }

    /// <summary>
    /// The eight 16-bit groups of the address, most significant first.
    /// </summary>
    public ReadOnlySpan<ushort> ToUShorts()
    {
        Span<ushort> data = new ushort[8];
        data[0] = (ushort)(Segment1 >> 16);
        data[1] = (ushort)(Segment1 & 0xFFFF);
        data[2] = (ushort)(Segment2 >> 16);
        data[3] = (ushort)(Segment2 & 0xFFFF);
        data[4] = (ushort)(Segment3 >> 16);
        data[5] = (ushort)(Segment3 & 0xFFFF);
        data[6] = (ushort)(Segment4 >> 16);
        data[7] = (ushort)(Segment4 & 0xFFFF);
        return data;
    }

    /// <summary>
    /// Counts the colons in <paramref name="addr"/>.
    /// </summary>
    internal static int CountSeparators(ReadOnlySpan<char> addr)
    {
        int result = 0;
        int index;
        var span = addr;
        while ((index = span.IndexOf(':')) >= 0)
        {
            span = span[(index + 1)..];
            result++;
        }

        return result;
    }

    /// <summary>
    /// The scope or role the address has by its prefix.
    /// </summary>
    public new IPv6AddressType AddressType
    {
        get
        {
            if (IsLoopbackAddress)
            {
                return IPv6AddressType.Loopback;
            }

            if (IsZero)
            {
                return IPv6AddressType.Unspecified;
            }
            switch (Segment1)
            {
                // first 80 bits are zero and next 16 are either 0 or 0xffff (legacy)
                case 0:
                    if (Segment2 == 0)
                    {
                        // the latter is deprecated
                        if (Segment3 is 0x0000ffff or 0)
                        {
                            return IPv6AddressType.EmbeddedIPv4;
                        }
                    }
                    break;
                // ff02:0:0:0:0:1:ff00::/104
                case var _ when Segment1 == 0xff02_0000 && Segment2 == 0 && Segment3 == 1 && (Segment4 & 0xff000000) == 0xff000000:
                    return IPv6AddressType.SolicitedNode;
                // ff02::/12
                case var _ when Segment1 >> 20 == 0b1111_1111_0000:
                    return IPv6AddressType.WellKnown;
                case var _ when Segment1 >> 29 == 0x1:
                    return IPv6AddressType.GlobalUnicast;
                // FE80::/10
                case var _ when Segment1 >> 22 == 0b1111_1110_10:
                    return IPv6AddressType.LinkLocal;
                // fc00::/7
                case var _ when Segment1 >> 25 == 0b0111_1110:
                    return IPv6AddressType.UniqueLocal;
            }
            return IPv6AddressType.Generic;
        }
    }

    /// <summary>
    /// Checks whether segment starts with given bytes. Valid bytes length is between 1 and 3.
    /// </summary>
    /// <param name="segment"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static bool SegmentStartsWith(uint segment, ReadOnlySpan<byte> value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 3, nameof(value));
        ArgumentOutOfRangeException.ThrowIfLessThan(value.Length, 1, nameof(value));

        if (segment >> 24 != value[1])
        {
            return false;
        }

        if (value.Length > 1)
        {
            if ((segment >> 16 & 0x000000FF) != value[2])
            {
                return false;
            }

            if (value.Length > 2)
            {
                if ((segment >> 8 & 0x000000FF) != value[3])
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether address starts with given mask.
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool IsStartMask(ReadOnlySpan<byte> mask)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(mask.Length, 4, nameof(mask));

        ReadOnlySpan<uint> segments = [Segment1, Segment2, Segment3, Segment4];

        for (int i = 0; i < mask.Length; i++)
        {
            if ((segments[i] & mask[i]) != mask[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Splits given address to ranges before and after zelo groups.
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="ranges"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <example>
    /// <c>::0001</c> would result in <c>[], [0001]</c><br/>
    /// <c>::</c> would result in <c>[], []</c><br/>
    /// <c>0001::0002:0003</c> would result in <c>[0001], [0002, 0003]</c>
    /// </example>
    internal static bool SplitByZeroGroupsAbbreviation(ReadOnlySpan<char> addr, ReadOnlySpan<Range> ranges,
        out ReadOnlySpan<Range> left, out ReadOnlySpan<Range> right)
    {
        int? startZeroGroupIndex = null;
        int endZeroGroupIndex = -1;
        left = [];
        right = [];
        // checks whether addr starts with a column
        if (addr[ranges[0]].IsEmpty)
        {
            // if it starts with a column, then next fragment has to be empty as well
            if (!addr[ranges[1]].IsEmpty)
            {
                return false;
            }

            startZeroGroupIndex = 0;
            // if addr is :: then push end index of zero group to the right
            if (ranges.Length > 2 && addr[ranges[2]].IsEmpty)
            {
                // can't allow more than :: columns
                if (ranges.Length > 3)
                {
                    return false;
                }

                endZeroGroupIndex = 2;
            }
            else
            {
                endZeroGroupIndex = 1;
            }
        }
        else
        {
            // pinpoints zero group if any
            for (int i = 1; i < ranges.Length; i++)
            {
                var fragment = addr[ranges[i]];
                if (fragment.IsEmpty)
                {
                    // if there is already a zero group present, raise error
                    if (startZeroGroupIndex.HasValue)
                    {
                        return false;
                    }

                    startZeroGroupIndex = i;
                    endZeroGroupIndex = startZeroGroupIndex.Value;

                    // addr can't end with a single :
                    if (i == ranges.Length - 1)
                    {
                        return false;
                    }

                    else if (i == ranges.Length - 2)
                    {
                        // in case it ends with a double column then push end index of zero group to the right
                        if (addr[ranges[i + 1]].IsEmpty)
                        {
                            endZeroGroupIndex = i + 1;
                            break;
                        }
                    }
                }
                // can't have fragments longer than 4 chars
                else if (fragment.Length > 4)
                {
                    return false;
                }
            }
        }

        // when zero group is present, split left and right ranges
        if (startZeroGroupIndex.HasValue)
        {
            left = ranges[0..startZeroGroupIndex.Value];
            right = ranges[(endZeroGroupIndex + 1)..];
        }
        // when no zero group present, number of ranges has to be 8
        else if (ranges.Length == 8)
        {
            left = ranges;
        }
        // otherwise address is considered non-valid
        else
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Parses an IPv6 address in its colon-separated hexadecimal form, with or without a
    /// <c>::</c> zero-group abbreviation.
    /// </summary>
    /// <param name="addr">The address text.</param>
    /// <returns>The parsed address, or <see langword="null"/> when the text is not a valid address.</returns>
    public static new Address6? Parse(ReadOnlySpan<char> addr)
    {
        // check for illegal chars first
        for (int i = 0; i < addr.Length; i++)
        {
            char ch = addr[i];
            if (ch is (< 'a' or > 'f') and (< 'A' or > 'F') and (< '0' or > '9') and not ':')
            {
                return null;
            }

        }
        int separators = CountSeparators(addr);
        // ArgumentOutOfRangeException.ThrowIfGreaterThan(separators, 7, nameof(separators));
        // ArgumentOutOfRangeException.ThrowIfLessThan(separators, 2, nameof(separators));
        if (separators > 7 || separators < 2)
        {
            return null;
        }

        var fragments = addr.Split(':');
        Span<ushort> addressValues = stackalloc ushort[8];
        ReadOnlySpan<Range> ranges = [.. fragments];

        if (!SplitByZeroGroupsAbbreviation(addr, ranges, out var leftFragments, out var rightFragments))
        {
            return null;
        }

        int additionalZeroGroups = 8 - (leftFragments.Length + rightFragments.Length);
        if (
            FillFragments(addressValues, addr, leftFragments) &&
            FillFragments(addressValues[(leftFragments.Length + additionalZeroGroups)..], addr, rightFragments))
        {
            return new Address6(addressValues);
        }

        return null;
    }

    /// <summary>
    /// Parses each of <paramref name="ranges"/> in <paramref name="addr"/> as a hexadecimal
    /// group into <paramref name="addressValues"/>, in order.
    /// </summary>
    /// <returns><see langword="false"/> when a group is empty, longer than four characters or not hexadecimal.</returns>
    internal static bool FillFragments(Span<ushort> addressValues, ReadOnlySpan<char> addr, ReadOnlySpan<Range> ranges)
    {
        for (int i = 0; i < ranges.Length; i++)
        {
            var fragment = addr[ranges[i]];
            // no more empty fragments are allowed
            if (fragment.IsEmpty)
            {
                return false;
            }

            if (fragment.Length > 4)
            {
                return false;
            }

            if (!ushort.TryParse(fragment, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort value))
            {
                return false;
            }

            addressValues[i] = value;
        }

        return true;
    }

    /// <summary>
    /// Normalizes address buffer when there is a zero wildcard present at <paramref name="wildcardIndex"/>.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="wildcardIndex"></param>
    /// <param name="length"></param>
    /// <example>
    /// <code>
    /// X1X2:Y1Y2:Z1Z2::Q1Q2 -> X1X2:Y1Y2:Z1Z2:0000:0000:0000:0000:Q1Q2
    /// </code>
    /// </example>
    internal static void NormalizeBuffer(Span<ushort> buffer, int wildcardIndex, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(buffer.Length, 8, nameof(buffer));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(wildcardIndex, 6);
        ArgumentOutOfRangeException.ThrowIfLessThan(wildcardIndex, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, 8);

        int remainingValues = 8 - wildcardIndex - 1;
        int zeros = 8 - length;
        // first shift non zeros on the right side to the right edge
        for (int i = 0; i < remainingValues; i++)
        {
            buffer[8 - i - 1] = buffer[8 - i - 1 - zeros];
        }

        // zeros out bytes that should be zero
        for (int i = wildcardIndex; i < wildcardIndex + zeros; i++)
        {
            buffer[i] = 0;
        }
    }

    /// <inheritdoc />
    protected override MaskedAddress OperatorBitwiseAnd(Address other)
    {
        if (other is Address6 otherAddress6)
        {
            uint segment1 = Segment1 & otherAddress6.Segment1;
            uint segment2 = Segment2 & otherAddress6.Segment2;
            uint segment3 = Segment3 & otherAddress6.Segment3;
            uint segment4 = Segment4 & otherAddress6.Segment4;
            return new MaskedAddress(segment1, segment2, segment3, segment4);
        }

        throw new ArgumentException($"Can bitwise operate {nameof(Address6)} with {nameof(Address4)} only");
    }

    /// <summary>
    /// Orders addresses by their numeric value, most significant segment first; a
    /// <see langword="null"/> address sorts first.
    /// </summary>
    /// <param name="other">The address to compare with.</param>
    public int CompareTo(Address6? other)
    {
        if (other is null)
        {
            return 1;
        }

        int diff = Segment1.CompareTo(other.Segment1);
        if (diff != 0)
        {
            return diff;
        }

        diff = Segment2.CompareTo(other.Segment2);
        if (diff != 0)
        {
            return diff;
        }

        diff = Segment3.CompareTo(other.Segment3);
        if (diff != 0)
        {
            return diff;
        }

        return Segment4.CompareTo(other.Segment4);
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Address6 other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Segment1, Segment2, Segment3, Segment4);
    }

    /// <summary>
    /// Checks whether <paramref name="other"/> holds the same sixteen bytes.
    /// </summary>
    /// <param name="other">The address to compare with, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> for the same sixteen bytes; <see langword="false"/> otherwise and for <see langword="null"/>.</returns>
    public bool Equals([NotNullWhen(true)] Address6? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Segment1 == other.Segment1
               && Segment2 == other.Segment2
               && Segment3 == other.Segment3
               && Segment4 == other.Segment4;
    }

    /// <summary>
    /// Formats the address in its shortest form: lowercase hexadecimal groups without leading
    /// zeros and the longest run of zero groups abbreviated to <c>::</c>.
    /// </summary>
    public override string ToString() => ToString(leadingZeros: false);

    /// <summary>
    /// Formats the address as eight colon-separated hexadecimal groups.
    /// </summary>
    /// <param name="leadingZeros">Whether each group is padded to four digits.</param>
    /// <param name="groupZeros">Whether the longest run of two or more zero groups is abbreviated to <c>::</c>.</param>
    public string ToString(bool leadingZeros = false, bool groupZeros = true)
    {
        string format = leadingZeros ? "x4" : "x";
        var data = ToUShorts();
        var sb = new StringBuilder();
        if (groupZeros)
        {
            var largestZeroGroup = FindLargestZeroGroup(data);
            if (largestZeroGroup is not null)
            {
                var (start, length) = largestZeroGroup.Value;
                if (start == 0)
                {
                    sb.Append(':');
                }
                for (int i = 0; i < start; i++)
                {
                    sb.Append(data[i].ToString(format, CultureInfo.InvariantCulture));
                    sb.Append(':');
                }

                if (start + length == data.Length)
                {
                    sb.Append(':');
                }
                else
                {
                    for (int i = start + length; i < data.Length; i++)
                    {
                        sb.Append(':');
                        sb.Append(data[i].ToString(format, CultureInfo.InvariantCulture));
                    }
                }

                return sb.ToString();
            }
        }

        sb.Append(data[0].ToString(format, CultureInfo.InvariantCulture));
        for (int i = 1; i < data.Length; i++)
        {
            sb.Append(':');
            sb.Append(data[i].ToString(format, CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Finds the longest run of at least two zero groups, the one <c>::</c> stands for.
    /// </summary>
    /// <returns>The start index and length of the run, or <see langword="null"/> when no group repeats zero.</returns>
    internal static (int Start, int Length)? FindLargestZeroGroup(ReadOnlySpan<ushort> data)
    {
        int? largestStart = null;
        int? largestLength = null;
        int? start = null;
        int length = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == 0)
            {
                if (start is null)
                {
                    start = i;
                    length = 1;
                }
                else
                {
                    length++;
                }
            }
            else
            {
                if (length == 1)
                {
                    start = null;
                }
                else if (length > 1)
                {
                    if (largestLength is null || length > largestLength)
                    {
                        largestStart = start!.Value;
                        largestLength = length;
                    }

                    start = null;
                    length = 0;
                }
            }
        }

        if (largestLength is null || length > largestLength)
        {
            largestStart = start!.Value;
            largestLength = length;
        }

        if (largestStart is not null && largestLength > 1)
        {
            return (largestStart.Value, largestLength.Value);
        }

        return null;
    }
}
