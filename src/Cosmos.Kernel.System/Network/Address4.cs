// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Represents an IPv4 address.
/// </summary>
public sealed class Address4 : Address, IComparable<Address4>, IEquatable<Address4>
{
    /// <summary>
    /// The <c>0.0.0.0</c> IP address.
    /// </summary>
    public static readonly Address4 Zero = new(0x00000000);

    /// <summary>
    /// The broadcast address <c>(255.255.255.255)</c>.
    /// </summary>
    public static Address4 Broadcast { get; } = new(0xFFFFFFFF);

    /// <summary>
    /// The four octets packed into one number, the first octet in the most
    /// significant byte.
    /// </summary>
    public uint Segment1 { get; }

    /// <inheritdoc />
    public override MaskedAddress Parts => new(Segment1);

    /// <inheritdoc />
    public override bool IsZero => Equals(Zero);

    /// <summary>
    /// Whether this is the limited broadcast address <c>255.255.255.255</c>.
    /// </summary>
    public override bool IsBroadcastAddress => Equals(Broadcast);

    /// <summary>
    /// Whether this is a loopback address, one in <c>127.0.0.0/8</c>.
    /// </summary>
    public override bool IsLoopbackAddress => (Segment1 >> 24) == 127;

    /// <summary>
    /// Creates a new <see cref="Address4"/> instance from the packed address.
    /// </summary>
    /// <param name="address">The four octets packed into one number, the first octet in the most significant byte.</param>
    public Address4(uint address) => Segment1 = address;

    /// <summary>
    /// Creates a new <see cref="Address4"/> instance from its four octets.
    /// </summary>
    /// <param name="first">First block of the address.</param>
    /// <param name="second">Second block of the address.</param>
    /// <param name="third">Third block of the address.</param>
    /// <param name="fourth">Fourth block of the address.</param>
    public Address4(byte first, byte second, byte third, byte fourth) => Segment1 = ToUint32(first, second, third, fourth);

    /// <summary>
    /// Creates a new <see cref="Address4"/> instance from four bytes of <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The array holding the address, most significant byte first.</param>
    /// <param name="offset">The index of the address's first byte in <paramref name="buffer"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or leaves fewer than four bytes after it.</exception>
    public Address4(byte[] buffer, int offset) : this(new ReadOnlySpan<byte>(buffer, offset, 4))
    {
    }

    /// <summary>
    /// Creates a new <see cref="Address4"/> instance, with the specified byte span.
    /// </summary>
    /// <param name="buffer">The four address bytes, most significant first.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="buffer"/> is not exactly four bytes long.</exception>
    public Address4(ReadOnlySpan<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(buffer.Length, 4, nameof(buffer));

        Segment1 = ToUint32(buffer);
    }

    /// <summary>
    /// Parses a dotted IPv4 address, such as <c>192.168.1.1</c>.
    /// </summary>
    /// <param name="addr">The address text.</param>
    /// <param name="style">The number base of the four octets.</param>
    /// <returns>The parsed address, or <see langword="null"/> when the text is not four octets in that base.</returns>
    public static Address4? Parse(ReadOnlySpan<char> addr, AddressNumericStyle style)
    {
        Span<byte> addressBytes = stackalloc byte[4];

        int index = 0;
        bool isGood = false;
        NumberStyles numberStyles = style == AddressNumericStyle.Dec ? NumberStyles.Number : NumberStyles.HexNumber;
        foreach (Range fragment in addr.Split('.'))
        {
            // A fifth fragment means more than four octets: not an IPv4 address.
            if (index > 3)
            {
                return null;
            }
            if (!byte.TryParse(addr[fragment], numberStyles, CultureInfo.InvariantCulture, out byte value))
            {
                return null;
            }

            addressBytes[index++] = value;
            if (index == 4)
            {
                isGood = true;
            }
        }

        return isGood ? new Address4(addressBytes) : null;
    }

    /// <summary>
    /// Convert a CIDR number to an IPv4 address.
    /// </summary>
    /// <param name="cidr">The CIDR number.</param>
    /// <returns>The subnet mask with <paramref name="cidr"/> leading one bits, or <see langword="null"/> when it cannot be built.</returns>
    // ReSharper disable once InconsistentNaming
    public static Address4? CIDRToAddress(int cidr)
    {
        try
        {
            uint mask = 0xffffffff << (32 - cidr);
            return new Address4((byte)(mask >> 24), (byte)(mask >> 16 & 0xff), (byte)(mask >> 8 & 0xff), (byte)(mask & 0xff));
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public override ReadOnlySpan<byte> ToBytes()
    {
        Span<byte> data = new byte[4];
        SegmentToSpan(Segment1, data);
        return data;
    }

    /// <summary>
    /// Whether this is an APIPA (link-local) address, one in <c>169.254.0.0/16</c>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool IsAPIPA() => (Segment1 >> 16) == 0xA9_FE; // 169, 254

    /// <summary>
    /// Formats the address in dotted-decimal notation (e.g. <c>192.168.1.1</c>).
    /// </summary>
    public override string ToString() => ToString(AddressNumericStyle.Dec);

    /// <summary>
    /// Orders addresses by their numeric value (<see cref="Segment1"/>); a
    /// <see langword="null"/> address sorts first.
    /// </summary>
    /// <param name="other">The address to compare with.</param>
    public int CompareTo(Address4? other) => other is null ? 1 : Segment1.CompareTo(other.Segment1);

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj) || obj is Address4 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Segment1);

    /// <inheritdoc />
    protected override MaskedAddress OperatorBitwiseAnd(Address other)
    {
        if (other is Address4 otherAddress4)
        {
            uint segment1 = Segment1 & otherAddress4.Segment1;
            return new MaskedAddress(segment1);
        }

        throw new ArgumentException($"Can bitwise operate {nameof(Address4)} with {nameof(Address4)} only");
    }

    /// <summary>
    /// Checks whether <paramref name="other"/> holds the same four bytes.
    /// </summary>
    /// <param name="other">The address to compare with, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> for the same four bytes; <see langword="false"/> otherwise and for <see langword="null"/>.</returns>
    public bool Equals([NotNullWhen(true)] Address4? other) => other is not null && Segment1 == other.Segment1;

    /// <summary>
    /// Formats the address as four dotted octets in the given number base.
    /// </summary>
    /// <param name="numericStyle">The number base of each octet.</param>
    /// <param name="leadingZeros">Whether each octet is padded to its full width: three decimal digits or two hexadecimal ones.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="numericStyle"/> is not a known style.</exception>
    public string ToString(AddressNumericStyle numericStyle = AddressNumericStyle.Dec, bool leadingZeros = false)
    {
        string format = numericStyle switch
        {
            AddressNumericStyle.Hex => leadingZeros ? "x2" : "x",
            AddressNumericStyle.Dec => leadingZeros ? "000" : "",
            _ => throw new ArgumentOutOfRangeException(nameof(numericStyle), numericStyle, null)
        };
        Span<byte> data = stackalloc byte[4];
        SegmentToSpan(Segment1, data);
        return
            $"{data[0].ToString(format, CultureInfo.InvariantCulture)}.{data[1].ToString(format, CultureInfo.InvariantCulture)}.{data[2].ToString(format, CultureInfo.InvariantCulture)}.{data[3].ToString(format, CultureInfo.InvariantCulture)}";
    }
}
