// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv6;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The internet layer of a frame: an IPv4 or an IPv6 packet. A transport
/// protocol composes one of these instead of deriving from it, which is what
/// lets a single transport class serve both versions: the internet packet
/// owns the frame buffer, reports where the transport section starts and how
/// long it is, computes the transport checksum with its own pseudo-header,
/// and queues itself on the outgoing buffer that resolves its kind of
/// neighbor address.
/// </summary>
/// <remarks>
/// A class has one base, so a transport class that derived from the IPv4
/// packet could never also derive from the IPv6 one. Everything a transport
/// needs from the layer below it is declared here, and the version-specific
/// half lives in the two overrides.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public abstract class InternetPacket : EthernetPacket
{
    /// <summary>EtherType of an IPv4 frame.</summary>
    internal const ushort EtherTypeIPv4 = 0x0800;

    /// <summary>EtherType of an IPv6 frame.</summary>
    internal const ushort EtherTypeIPv6 = 0x86DD;

    /// <summary>IP protocol number of ICMP.</summary>
    internal const byte ProtocolIcmp = 1;

    /// <summary>IP protocol number of TCP.</summary>
    internal const byte ProtocolTcp = 6;

    /// <summary>IP protocol number of UDP.</summary>
    internal const byte ProtocolUdp = 17;

    /// <summary>Next-header value of ICMPv6.</summary>
    internal const byte ProtocolIcmpv6 = 58;

    /// <summary>
    /// Hop limit a locally built IPv6 transport packet carries. IPv4 keeps
    /// its own TTL, written by the IPv4 build constructors.
    /// </summary>
    private const byte TransportHopLimit = 64;

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied.
    /// </summary>
    /// <param name="rawData">The raw frame bytes, starting at the Ethernet header.</param>
    private protected InternetPacket(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Initializes a new instance, allocating the frame buffer and writing
    /// the Ethernet header.
    /// </summary>
    /// <param name="destinationMac">Destination MAC address.</param>
    /// <param name="sourceMac">Source MAC address.</param>
    /// <param name="etherType">EtherType of the frame.</param>
    /// <param name="packetSize">Total frame size in bytes.</param>
    private protected InternetPacket(MACAddress destinationMac, MACAddress sourceMac, ushort etherType, int packetSize)
        : base(destinationMac, sourceMac, etherType, packetSize)
    {
    }

    /// <summary>
    /// The source address.
    /// </summary>
    public abstract Address SourceIP { get; }

    /// <summary>
    /// The destination address.
    /// </summary>
    public abstract Address DestinationIP { get; }

    /// <summary>
    /// The offset of the transport section from the start of the frame: the
    /// Ethernet header plus the internet header.
    /// </summary>
    public abstract ushort DataOffset { get; }

    /// <summary>
    /// The length of the transport section in bytes, header included.
    /// </summary>
    public abstract ushort DataLength { get; }

    /// <summary>
    /// Whether this version requires every transport section to carry a
    /// checksum. IPv6 does, having dropped the header checksum of IPv4, so a
    /// zero checksum field is a corrupt datagram rather than an absent one.
    /// </summary>
    internal abstract bool TransportChecksumRequired { get; }

    /// <summary>
    /// Computes a transport checksum over the section at
    /// <see cref="DataOffset"/>: the ones' complement of the sum of this
    /// version's pseudo-header and <paramref name="length"/> payload bytes.
    /// Run over a section whose checksum field holds zero it yields the value
    /// to store; run over a received section it yields zero when that section
    /// is intact.
    /// </summary>
    /// <param name="protocol">The protocol number to put in the pseudo-header.</param>
    /// <param name="length">The transport length: the whole section, header included.</param>
    internal abstract ushort ComputeTransportChecksum(byte protocol, ushort length);

    /// <summary>
    /// Queues this packet on the outgoing buffer of its address family, which
    /// resolves the destination MAC address: ARP for IPv4, Neighbor Discovery
    /// for IPv6. The queue is not pumped here.
    /// </summary>
    /// <returns>False when no configured interface carries the packet's source address.</returns>
    internal abstract bool Enqueue();

    /// <summary>
    /// Parses the internet layer of a received frame, picking the version
    /// from the EtherType. The array is aliased, not copied, and no length
    /// field is validated.
    /// </summary>
    /// <param name="rawData">The raw frame bytes, starting at the Ethernet header.</param>
    /// <returns>The parsed packet.</returns>
    /// <exception cref="ArgumentException">The frame is shorter than an Ethernet
    /// header, or carries neither an IPv4 nor an IPv6 EtherType.</exception>
    internal static InternetPacket Parse(byte[] rawData)
    {
        if (rawData.Length < 14)
        {
            throw new ArgumentException("The frame is too short to hold an Ethernet header.", nameof(rawData));
        }

        ushort etherType = (ushort)((rawData[12] << 8) | rawData[13]);

        return etherType switch
        {
            EtherTypeIPv4 => new IPPacket(rawData),
            EtherTypeIPv6 => new IPv6Packet(rawData),
            _ => throw new ArgumentException("The frame carries neither an IPv4 nor an IPv6 header.", nameof(rawData))
        };
    }

    /// <summary>
    /// Builds the internet packet that carries a transport section, of the
    /// version both addresses belong to. The source MAC address is resolved
    /// from the device configured with <paramref name="source"/>; the
    /// destination MAC address is left for the outgoing buffer to resolve,
    /// except for an IPv6 multicast destination, which maps to one directly.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    /// <param name="protocol">Protocol number of the transport section.</param>
    /// <param name="payloadLength">Length of the transport section in bytes, header included.</param>
    /// <param name="dontFragment">Whether to set the IPv4 Don't Fragment flag. Ignored for IPv6, which routers never fragment.</param>
    /// <returns>The built packet, with the transport section left as zeroes.</returns>
    /// <exception cref="ArgumentException">The two addresses belong to different versions.</exception>
    internal static InternetPacket CreateForTransport(Address source, Address destination, byte protocol,
        ushort payloadLength, bool dontFragment)
    {
        RequireSameFamily(source, destination);

        return source is Address6 source6
            ? new IPv6Packet(payloadLength, protocol, TransportHopLimit, source6, (Address6)destination)
            : new IPPacket(payloadLength, protocol, source, destination, FragmentFlags(dontFragment));
    }

    /// <summary>
    /// Builds the internet packet that carries a transport section with a
    /// destination MAC address already known, which skips neighbor
    /// resolution. Used for a broadcast destination, which has no neighbor to
    /// resolve.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    /// <param name="protocol">Protocol number of the transport section.</param>
    /// <param name="payloadLength">Length of the transport section in bytes, header included.</param>
    /// <param name="dontFragment">Whether to set the IPv4 Don't Fragment flag. Ignored for IPv6, which routers never fragment.</param>
    /// <param name="destinationMac">Destination MAC address to write into the Ethernet header.</param>
    /// <returns>The built packet, with the transport section left as zeroes.</returns>
    /// <exception cref="ArgumentException">The two addresses belong to different versions.</exception>
    internal static InternetPacket CreateForTransport(Address source, Address destination, byte protocol,
        ushort payloadLength, bool dontFragment, MACAddress destinationMac)
    {
        RequireSameFamily(source, destination);

        return source is Address6 source6
            ? new IPv6Packet(payloadLength, protocol, TransportHopLimit, source6, (Address6)destination, destinationMac)
            : new IPPacket(payloadLength, protocol, source, destination, FragmentFlags(dontFragment), destinationMac);
    }

    /// <summary>
    /// The raw value of IPv4 header byte 20: the 3 flag bits followed by the
    /// upper 5 bits of the fragment offset, which is always zero here.
    /// </summary>
    private static byte FragmentFlags(bool dontFragment) => dontFragment ? (byte)0x40 : (byte)0x00;

    /// <summary>
    /// Throws when the two addresses are not of the same IP version, which no
    /// single packet can carry.
    /// </summary>
    private static void RequireSameFamily(Address source, Address destination)
    {
        if (source.AddressFamily != destination.AddressFamily)
        {
            throw new ArgumentException(
                $"The source address is {source.AddressFamily} and the destination address is {destination.AddressFamily}; one packet carries a single version.",
                nameof(destination));
        }
    }

    /// <summary>
    /// Sums a range of <paramref name="buffer"/> as big-endian 16-bit words,
    /// a trailing odd byte padded with zero, without folding the carries.
    /// </summary>
    /// <param name="buffer">The buffer to read.</param>
    /// <param name="offset">The offset to start at, in bytes.</param>
    /// <param name="length">The number of bytes to sum.</param>
    internal static uint SumWords(byte[] buffer, int offset, int length)
    {
        uint sum = 0;
        int end = offset + (length & ~1);

        for (int i = offset; i < end; i += 2)
        {
            sum += (uint)((buffer[i] << 8) | buffer[i + 1]);
        }

        if ((length & 1) != 0)
        {
            sum += (uint)(buffer[end] << 8);
        }

        return sum;
    }

    /// <summary>
    /// Folds the carries of an accumulated checksum sum back into 16 bits.
    /// </summary>
    /// <param name="sum">The unfolded sum, as <see cref="SumWords"/> returns.</param>
    internal static ushort Fold(uint sum)
    {
        while ((sum >> 16) != 0)
        {
            sum = (sum & 0xFFFF) + (sum >> 16);
        }

        return (ushort)sum;
    }

    /// <summary>
    /// Computes the Internet ones' complement checksum over a range of the
    /// given buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read.</param>
    /// <param name="offset">The offset to start at, in bytes.</param>
    /// <param name="length">The number of bytes to sum.</param>
    internal static ushort CalcOcCrc(byte[] buffer, int offset, int length)
    {
        return (ushort)~Fold(SumWords(buffer, offset, length));
    }
}
