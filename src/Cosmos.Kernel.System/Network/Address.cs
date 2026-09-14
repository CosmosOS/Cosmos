/*
 * PROJECT:          Aura Operating System Development
 * CONTENT:          IP Address
 * PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
 *                   Port of Cosmos Code.
 */

using System.Runtime.CompilerServices;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv6;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The IP version an <see cref="Address"/> belongs to.
/// </summary>
public enum AddressFamily
{
    /// <summary>
    /// A 32-bit IPv4 address, an <see cref="Address4"/>.
    /// </summary>
    IPv4,

    /// <summary>
    /// A 128-bit IPv6 address, an <see cref="Address6"/>.
    /// </summary>
    IPv6
}

/// <summary>
/// The number base an IPv4 address is parsed from or formatted in.
/// </summary>
public enum AddressNumericStyle
{
    /// <summary>
    /// Decimal octets, as in <c>192.168.1.1</c>.
    /// </summary>
    Dec,

    /// <summary>
    /// Hexadecimal octets, as in <c>c0.a8.1.1</c>.
    /// </summary>
    Hex
}

/// <summary>
/// Represents an IP address: an <see cref="Address4"/> or an <see cref="Address6"/>.
/// </summary>
#pragma warning disable CS0660, CS0661 // Equals and GetHashCode are implemented in sub-classes
public abstract class Address : IComparable<Address>
#pragma warning restore CS0660, CS0661
{
    /// <summary>
    /// Whether this is an IPv4 address.
    /// </summary>
    public bool IsIpv4 => this is Address4;

    /// <summary>
    /// Whether this is an IPv6 address.
    /// </summary>
    public bool IsIpv6 => !IsIpv4;

    /// <summary>
    /// Whether every bit of the address is zero: <c>0.0.0.0</c> or <c>::</c>.
    /// </summary>
    public abstract bool IsZero { get; }

    /// <summary>
    /// The address bytes, most significant first, indexable by octet.
    /// </summary>
    public abstract MaskedAddress Parts { get; }

    /// <summary>
    /// The IP version of this address.
    /// </summary>
    public AddressFamily AddressFamily => IsIpv6 ? AddressFamily.IPv6 : AddressFamily.IPv4;

    /// <summary>
    /// Whether this is the limited broadcast address <c>255.255.255.255</c>.
    /// Always <see langword="false"/> for IPv6, which has multicast instead.
    /// </summary>
    public abstract bool IsBroadcastAddress { get; }

    /// <summary>
    /// Parses an IP address in its string representation.
    /// </summary>
    /// <param name="addr">The IP address as string.</param>
    /// <returns>The parsed address value or null when parsing fails.</returns>
    public static Address? Parse(ReadOnlySpan<char> addr)
    {
        return Parse(addr) ?? Address6.Parse(addr);
    }

    /// <summary>
    /// Check if this address is a loopback address.
    /// </summary>
    public abstract bool IsLoopbackAddress { get; }

    /// <summary>
    /// Packs the first four bytes of <paramref name="buffer"/> into one number, the first byte
    /// in the most significant position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ToUint32(ReadOnlySpan<byte> buffer)
    {
        return ToUint32(buffer[0], buffer[1], buffer[2], buffer[3]);
    }

    /// <summary>
    /// Packs four octets into one number, <paramref name="first"/> in the most significant byte.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ToUint32(byte first, byte second, byte third, byte fourth)
    {
        return (uint)((first << 24) | (second << 16) | (third << 8) | fourth);
    }

    /// <summary>
    /// Writes the four bytes of <paramref name="segment"/> to <paramref name="destination"/>,
    /// most significant first.
    /// </summary>
    internal static void SegmentToSpan(uint segment, Span<byte> destination)
    {
        destination[0] = (byte)(segment >> 24);
        destination[1] = (byte)((segment >> 16) & 0xFF);
        destination[2] = (byte)((segment >> 8) & 0xFF);
        destination[3] = (byte)(segment & 0xFF);
    }

    /// <summary>
    /// The address bytes in network order: four for IPv4, sixteen for IPv6.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract ReadOnlySpan<byte> ToBytes();

    /// <summary>
    /// Orders two IPv4 addresses by their numeric value; a <see langword="null"/> address
    /// sorts first.
    /// </summary>
    /// <param name="other">The address to compare with.</param>
    /// <exception cref="ArgumentException">Either address is not an <see cref="Address4"/>.</exception>
    public int CompareTo(Address? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (this is Address4 address4 && other is Address4 otherAddress4)
        {
            return address4.CompareTo(otherAddress4);
        }

        throw new ArgumentException("Only addresses of same type can be compared", nameof(other));
    }

    /// <summary>
    /// Masks <paramref name="a"/> with <paramref name="b"/> bit by bit, as when a subnet mask
    /// is applied to an address.
    /// </summary>
    /// <param name="a">The address to mask.</param>
    /// <param name="b">The mask.</param>
    /// <exception cref="ArgumentException">The two addresses are not of the same family.</exception>
    public static MaskedAddress operator &(Address a, Address b)
    {
        return a.OperatorBitwiseAnd(b);
    }

    /// <summary>
    /// Masks this address with <paramref name="other"/> bit by bit.
    /// </summary>
    /// <param name="other">The mask, of the same family as this address.</param>
    /// <exception cref="ArgumentException"><paramref name="other"/> is not of the same family.</exception>
    protected abstract MaskedAddress OperatorBitwiseAnd(Address other);

    /// <summary>
    /// Checks whether two addresses hold the same bytes.
    /// </summary>
    /// <param name="a">The first address.</param>
    /// <param name="b">The second address.</param>
    public static bool operator ==(Address a, Address b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is Address4 a4 && b is Address4 b4)
        {
            return a4.Equals(b4);
        }
        return false;
    }

    /// <summary>
    /// Checks whether two addresses hold different bytes.
    /// </summary>
    /// <param name="a">The first address.</param>
    /// <param name="b">The second address.</param>
    public static bool operator !=(Address a, Address b) => !(a == b);
}

/// <summary>
/// The result of masking an address: its bytes, most significant first, indexable by octet.
/// A stack-only value, so it is never boxed or stored.
/// </summary>
#pragma warning disable CS0660, CS0661 // Equals and GetHashcode does not make sense here
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
    public bool Equals(MaskedAddress other) => Segment1 == other.Segment1 && Segment2 == other.Segment2 &&
                                               Segment3 == other.Segment3 && Segment4 == other.Segment4 &&
                                               AddressFamily == other.AddressFamily;

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
    public static bool operator !=(MaskedAddress a, MaskedAddress b) => !(a == b);

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
}
