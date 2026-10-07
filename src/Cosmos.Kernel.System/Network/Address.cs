// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
 * PROJECT:          Aura Operating System Development
 * CONTENT:          IP Address
 * PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
 *                   Port of Cosmos Code.
 */

using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.System.Network;

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
    /// Whether this is a loopback address: any address in <c>127.0.0.0/8</c>, or <c>::1</c>.
    /// </summary>
    public abstract bool IsLoopbackAddress { get; }

    /// <summary>
    /// Parses an IP address in its string representation: dotted decimal for IPv4, colon
    /// separated hexadecimal for IPv6.
    /// </summary>
    /// <param name="addr">The IP address as string.</param>
    /// <returns>The parsed address value or null when parsing fails.</returns>
    public static Address? Parse(ReadOnlySpan<char> addr)
    {
        return Address4.Parse(addr, AddressNumericStyle.Dec) ?? (Address?)Address6.Parse(addr);
    }

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
    /// Orders two addresses of the same family by their numeric value; a
    /// <see langword="null"/> address sorts first.
    /// </summary>
    /// <param name="other">The address to compare with.</param>
    /// <exception cref="ArgumentException">The two addresses are not of the same family.</exception>
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

        if (this is Address6 address6 && other is Address6 otherAddress6)
        {
            return address6.CompareTo(otherAddress6);
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

        // Callers write `address != null` on the non-nullable type, so the
        // operator answers for a null operand rather than dereferencing it.
        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    /// <summary>
    /// Checks whether two addresses hold different bytes.
    /// </summary>
    /// <param name="a">The first address.</param>
    /// <param name="b">The second address.</param>
    public static bool operator !=(Address a, Address b)
    {
        return !(a == b);
    }
}
