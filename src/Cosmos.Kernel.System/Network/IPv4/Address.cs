/*
 * PROJECT:          Aura Operating System Development
 * CONTENT:          IP Address
 * PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
 *                   Port of Cosmos Code.
 */

using System.Collections.Immutable;

namespace Cosmos.Kernel.System.Network.IPv4;

/// <summary>
/// Represents a IPv4 address.
/// </summary>
public sealed class Address : IComparable<Address>
{
    private uint _id;

    /// <summary>
    /// The parts of the address.
    /// </summary>
    public ImmutableArray<byte> Parts { get; }

    /// <summary>
    /// The <c>0.0.0.0</c> IP address.
    /// </summary>
    public static readonly Address Zero = new(0, 0, 0, 0);

    /// <summary>
    /// The broadcast address <c>(255.255.255.255)</c>.
    /// </summary>
    public static readonly Address Broadcast = new(255, 255, 255, 255);

    /// <summary>
    /// Create new instance of the <see cref="Address"/> class, with specified IP address.
    /// </summary>
    /// <param name="address">Address</param>
    public Address(uint address)
    {
        Parts =
        [
            (byte)((address >> 24) & 0xFF),
            (byte)((address >> 16) & 0xFF),
            (byte)((address >> 8) & 0xFF),
            (byte)(address & 0xFF)
        ];
    }

    /// <summary>
    /// Create new instance of the <see cref="Address"/> class, with specified IP address.
    /// </summary>
    /// <param name="first">First block of the address.</param>
    /// <param name="second">Second block of the address.</param>
    /// <param name="third">Third block of the address.</param>
    /// <param name="fourth">Fourth block of the address.</param>
    public Address(byte first, byte second, byte third, byte fourth)
    {
        Parts = [first, second, third, fourth];
    }

    /// <summary>
    /// Create new instance of the <see cref="Address"/> class, with specified buffer and offset.
    /// </summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="offset">Offset.</param>
    public Address(byte[] buffer, int offset) : this(new ReadOnlySpan<byte>(buffer, offset, 4))
    {
    }

    /// <summary>
    /// Creates a new <see cref="Address"/> instance, with the specified byte span.
    /// </summary>
    /// <param name="buffer">The four address bytes, most significant first.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="buffer"/> is not exactly four bytes long.</exception>
    public Address(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length != 4)
        {
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer has to be 4 bytes long");
        }

        Parts = [.. buffer[0..4]];

    }

    /// <summary>
    /// Whether this is the limited broadcast address 255.255.255.255.
    /// Internal: its one caller is the outgoing queue, which uses it to skip
    /// ARP resolution.
    /// </summary>
    internal bool IsBroadcastAddress() =>
        Parts[0] == 0xFF
        && Parts[1] == 0xFF
        && Parts[2] == 0xFF
        && Parts[3] == 0xFF;

    /// <summary>
    /// Formats the address in dotted-decimal notation (e.g. <c>192.168.1.1</c>).
    /// </summary>
    public override string ToString()
    {
        return $"{Parts[0]}.{Parts[1]}.{Parts[2]}.{Parts[3]}";
    }

    /// <summary>
    /// The address bytes in network order, as a span over <see cref="Parts"/>.
    /// Internal: every caller is the socket plug layer, which is framework
    /// plumbing. A kernel reads the octets from <see cref="Parts"/>.
    /// </summary>
    internal ReadOnlySpan<byte> ToSpan() => Parts.AsSpan();

    /// <summary>
    /// Convert this address to a 32-bit number. Internal: <see cref="Id"/> is
    /// the same value under the name the ring already uses for it.
    /// </summary>
    internal uint ToUInt32()
    {
        return (uint)((Parts[0] << 24) | (Parts[1] << 16) | (Parts[2] << 8) | (Parts[3] << 0));
    }

    /// <summary>
    /// The four octets packed into one number, the first octet in the most
    /// significant byte. Lossless, so two addresses share an <see cref="Id"/>
    /// only when they are equal, which is what makes it usable as a map key.
    /// </summary>
    public uint Id
    {
        get
        {
            if (_id == 0)
            {
                _id = ToUInt32();
            }

            return _id;
        }
    }

    /// <summary>
    /// Orders addresses by their numeric value (<see cref="Id"/>); a
    /// <see langword="null"/> address sorts first.
    /// </summary>
    /// <param name="other">The address to compare with.</param>
    public int CompareTo(Address? other)
    {
        if (other is null)
        {
            return 1;
        }

        return Id.CompareTo(other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is Address other)
        {
            return Parts.SequenceEqual(other.Parts);
        }

        return false; // obj is not an Address

    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}
