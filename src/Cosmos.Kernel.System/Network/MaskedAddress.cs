// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The result of masking an address: its bytes, most significant first, indexable by octet.
/// A stack-only value, so it is never boxed or stored.
/// </summary>
#pragma warning disable CS0660, CS0661 // A value that is never boxed has no use for object equality or a hash code
public readonly ref struct MaskedAddress : IEquatable<MaskedAddress>
#pragma warning restore CS0660, CS0661
{
    /// <summary>
    /// Bytes 0 to 3, the whole of an IPv4 address.
    /// </summary>
    public uint Segment1 { get; }

    /// <summary>
    /// Bytes 4 to 7; zero for IPv4.
    /// </summary>
    public uint Segment2 { get; }

    /// <summary>
    /// Bytes 8 to 11; zero for IPv4.
    /// </summary>
    public uint Segment3 { get; }

    /// <summary>
    /// Bytes 12 to 15; zero for IPv4.
    /// </summary>
    public uint Segment4 { get; }

    /// <summary>
    /// The IP version of the masked address, which sets how many bytes it holds.
    /// </summary>
    public AddressFamily AddressFamily { get; }

    /// <summary>
    /// Creates a masked IPv4 address.
    /// </summary>
    /// <param name="segment1">The four bytes, most significant first.</param>
    public MaskedAddress(uint segment1)
    {
        Segment1 = segment1;
        AddressFamily = AddressFamily.IPv4;
    }

    /// <summary>
    /// Creates a masked IPv6 address.
    /// </summary>
    /// <param name="segment1">Bytes 0 to 3, most significant first.</param>
    /// <param name="segment2">Bytes 4 to 7.</param>
    /// <param name="segment3">Bytes 8 to 11.</param>
    /// <param name="segment4">Bytes 12 to 15.</param>
    public MaskedAddress(uint segment1, uint segment2, uint segment3, uint segment4)
    {
        Segment1 = segment1;
        Segment2 = segment2;
        Segment3 = segment3;
        Segment4 = segment4;
        AddressFamily = AddressFamily.IPv6;
    }

    /// <summary>
    /// Checks whether <paramref name="other"/> holds the same bytes and family.
    /// </summary>
    /// <param name="other">The masked address to compare with.</param>
    public bool Equals(MaskedAddress other)
    {
        return Segment1 == other.Segment1 && Segment2 == other.Segment2 &&
               Segment3 == other.Segment3 && Segment4 == other.Segment4 &&
               AddressFamily == other.AddressFamily;
    }

    /// <summary>
    /// Checks whether two masked addresses hold the same bytes and family.
    /// </summary>
    /// <param name="a">The first masked address.</param>
    /// <param name="b">The second masked address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(MaskedAddress a, MaskedAddress b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Checks whether two masked addresses differ in bytes or family.
    /// </summary>
    /// <param name="a">The first masked address.</param>
    /// <param name="b">The second masked address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(MaskedAddress a, MaskedAddress b)
    {
        return !(a == b);
    }

    /// <summary>
    /// The byte at <paramref name="index"/>, counted from the most significant one.
    /// </summary>
    /// <param name="index">0 to 3 for IPv4, 0 to 15 for IPv6.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or past the last byte.</exception>
    public byte this[int index]
    {
        get
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(index)} can not be lower than zero");
            }
            int maxIndex = AddressFamily == AddressFamily.IPv4 ? 4 : 16;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, maxIndex, nameof(index));
            uint segment = (index / 4) switch
            {
                0 => Segment1,
                1 => Segment2,
                2 => Segment3,
                3 => Segment4,
                _ => throw new ArgumentOutOfRangeException($"Invalid {nameof(index)} of {index}"),
            };
            int part = index % 4;
            return (byte)(segment >> ((3 - part) * 8) & 0xFF);
        }
    }

    /// <summary>
    /// Not supported: a stack-only value is never boxed, so there is no object to compare with.
    /// </summary>
    /// <param name="obj">Ignored.</param>
    /// <exception cref="NotImplementedException">Always.</exception>
    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
