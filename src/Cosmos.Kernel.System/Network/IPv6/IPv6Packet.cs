// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.TCP;
using Cosmos.Kernel.System.Network.UDP;

namespace Cosmos.Kernel.System.Network.IPv6;

/// <summary>
/// An IPv6 packet over Ethernet (EtherType 0x86DD). The build constructors
/// write the complete 40-byte header at construction time. IPv6 has no
/// header checksum: the one checksum a derived type computes covers its own
/// section and the pseudo-header, see <see cref="CalcUpperLayerChecksum"/>.
/// Header properties are snapshots parsed from
/// <see cref="EthernetPacket.RawData"/> at construction time. Extension
/// headers are not parsed: the payload is taken to start at
/// <see cref="PayloadOffset"/>.
/// </summary>
internal class IPv6Packet : InternetPacket
{
    /// <summary>Length of the fixed IPv6 header, in bytes.</summary>
    internal const int HeaderLength = 40;

    /// <summary>Offset of the source address from the start of the frame.</summary>
    internal const int SourceOffset = 22;

    /// <summary>Offset of the destination address from the start of the frame.</summary>
    internal const int DestinationOffset = 38;

    /// <summary>
    /// Offset of the payload from the start of the frame: the Ethernet header
    /// plus the fixed IPv6 header. The same value as
    /// <see cref="InternetPacket.DataOffset"/>, kept as a constant because
    /// the ICMPv6 and Neighbor Discovery headers are laid out against it in
    /// constant expressions.
    /// </summary>
    internal const int PayloadOffset = 14 + HeaderLength;

    /// <summary>Parsed source address backing <see cref="SourceIP"/>.</summary>
    private Address6 _sourceIP = null!;

    /// <summary>Parsed destination address backing <see cref="DestinationIP"/>.</summary>
    private Address6 _destinationIP = null!;

    /// <summary>
    /// Handles a single IPv6 frame: ICMPv6, UDP and TCP are dispatched, and
    /// only when the destination is one of the stack's addresses or the
    /// solicited-node group of one; everything else is dropped. Extension
    /// headers are not parsed, so a frame that carries one reaches its
    /// transport handler with the extension header read as the transport
    /// header.
    /// </summary>
    /// <param name="packetData">The raw data of the frame.</param>
    internal static void IPv6Handler(byte[] packetData)
    {
        if (packetData.Length < PayloadOffset)
        {
            return;
        }

        IPv6Packet packet = new(packetData);

        Serial.WriteString("[IPv6] From ");
        Serial.WriteString(packet.SourceIP.ToString());
        Serial.WriteString(" to ");
        Serial.WriteString(packet.DestinationIP.ToString());
        Serial.WriteString(" next=");
        Serial.WriteNumber((ulong)packet.NextHeader);
        Serial.WriteString("\n");

        if (PayloadOffset + packet.PayloadLength > packetData.Length)
        {
            Serial.WriteString("[IPv6] Payload length past the frame, dropping\n");
            return;
        }

        if (!IsForUs(packet.DestinationIP))
        {
            Serial.WriteString("[IPv6] Packet not for us, dropping\n");
            return;
        }

        switch (packet.NextHeader)
        {
            case ProtocolIcmpv6 when packet.PayloadLength >= Icmpv6Packet.Icmpv6HeaderLength:
                Icmpv6Packet.Icmpv6Handler(packetData);
                break;
            case ProtocolUdp:
                UdpPacket.UDPHandler(packet);
                break;
            case ProtocolTcp:
                TcpPacket.TCPHandler(packet);
                break;
        }
    }

    /// <summary>
    /// Whether <paramref name="destination"/> is one of the stack's unicast
    /// addresses or the solicited-node multicast group of one.
    /// </summary>
    private static bool IsForUs(Address6 destination)
    {
        if (NetworkStack.AddressMap.ContainsKey(destination))
        {
            return true;
        }

        if (destination.AddressType != IPv6AddressType.SolicitedNode)
        {
            return false;
        }

        foreach (KeyValuePair<Address, INetworkDevice> pair in NetworkStack.AddressMap)
        {
            if (pair.Key is Address6 own && (own.Segment4 & 0x00FF_FFFF) == (destination.Segment4 & 0x00FF_FFFF))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The Ethernet address an IPv6 multicast group maps to: <c>33:33</c>
    /// followed by the low 32 bits of the group address (RFC 2464 section 7).
    /// </summary>
    /// <param name="group">The multicast group.</param>
    internal static MACAddress MulticastMac(Address6 group)
    {
        uint low = group.Segment4;
        return new MACAddress([0x33, 0x33, (byte)(low >> 24), (byte)(low >> 16), (byte)(low >> 8), (byte)low]);
    }

    /// <summary>
    /// The MAC of the device configured with <paramref name="source"/>, or
    /// <see cref="MACAddress.None"/> when no device carries it.
    /// </summary>
    private static MACAddress GetSourceMac(Address6 source)
    {
        return NetworkStack.AddressMap.TryGetValue(source, out INetworkDevice? device) ? device.MacAddress : MACAddress.None;
    }

    /// <summary>
    /// The destination MAC a build constructor can settle at once: the mapped
    /// address for a multicast group, <see cref="MACAddress.None"/> for a
    /// unicast destination that Neighbor Discovery resolves at send time.
    /// </summary>
    private static MACAddress GetDestinationMac(Address6 destination)
    {
        return destination.IsMulticast ? MulticastMac(destination) : MACAddress.None;
    }

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied, and the payload length is not validated.
    /// </summary>
    /// <param name="rawData">The raw data of the frame.</param>
    public IPv6Packet(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Initializes a new instance, resolving the source MAC from the device
    /// configured with <paramref name="source"/> and settling the destination
    /// MAC only for a multicast destination.
    /// </summary>
    /// <param name="payloadLength">Length of the payload following the fixed header, in bytes.</param>
    /// <param name="nextHeader">Protocol of the payload.</param>
    /// <param name="hopLimit">Hop limit.</param>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    internal IPv6Packet(ushort payloadLength, byte nextHeader, byte hopLimit, Address6 source, Address6 destination)
        : this(GetSourceMac(source), GetDestinationMac(destination), payloadLength, nextHeader, hopLimit, source, destination)
    {
    }

    /// <summary>
    /// Initializes a new instance with the destination MAC address already
    /// known, which skips Neighbor Discovery, resolving the source MAC
    /// address from the device configured with <paramref name="source"/>.
    /// </summary>
    /// <param name="payloadLength">Length of the payload following the fixed header, in bytes.</param>
    /// <param name="nextHeader">Protocol of the payload.</param>
    /// <param name="hopLimit">Hop limit.</param>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    /// <param name="destinationMac">Destination MAC address.</param>
    internal IPv6Packet(ushort payloadLength, byte nextHeader, byte hopLimit, Address6 source, Address6 destination, MACAddress destinationMac)
        : this(GetSourceMac(source), destinationMac, payloadLength, nextHeader, hopLimit, source, destination)
    {
    }

    /// <summary>
    /// Initializes a new instance, writing the complete IPv6 header: version
    /// 6, traffic class and flow label zero.
    /// </summary>
    /// <param name="sourceMac">Source MAC address.</param>
    /// <param name="destinationMac">Destination MAC address.</param>
    /// <param name="payloadLength">Length of the payload following the fixed header, in bytes.</param>
    /// <param name="nextHeader">Protocol of the payload.</param>
    /// <param name="hopLimit">Hop limit.</param>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    internal IPv6Packet(MACAddress sourceMac, MACAddress destinationMac, ushort payloadLength, byte nextHeader, byte hopLimit, Address6 source, Address6 destination)
        : base(destinationMac, sourceMac, EtherTypeIPv6, PayloadOffset + payloadLength)
    {
        RawData[14] = 0x60;
        RawData[15] = 0;
        RawData[16] = 0;
        RawData[17] = 0;
        RawData[18] = (byte)(payloadLength >> 8);
        RawData[19] = (byte)payloadLength;
        RawData[20] = nextHeader;
        RawData[21] = hopLimit;
        source.ToBytes().CopyTo(RawData.AsSpan(SourceOffset, 16));
        destination.ToBytes().CopyTo(RawData.AsSpan(DestinationOffset, 16));

        InitializeFields();
    }

    /// <summary>
    /// Parses the header fields from <see cref="EthernetPacket.RawData"/>, in
    /// addition to the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        PayloadLength = (ushort)((RawData[18] << 8) | RawData[19]);
        NextHeader = RawData[20];
        HopLimit = RawData[21];
        _sourceIP = new Address6(RawData, SourceOffset);
        _destinationIP = new Address6(RawData, DestinationOffset);
    }

    /// <summary>
    /// Computes the upper-layer checksum (RFC 8200 section 8.1): the ones'-complement
    /// sum of the pseudo-header (source, destination, upper-layer length,
    /// next header) and of <paramref name="length"/> bytes from
    /// <see cref="DataOffset"/>. Run over a section whose checksum field is
    /// zero it yields the value to store; run over a received section it
    /// yields zero when that section is intact.
    /// </summary>
    /// <param name="length">The upper-layer length: the whole IPv6 payload.</param>
    private protected ushort CalcUpperLayerChecksum(ushort length) => ComputeTransportChecksum(NextHeader, length);

    /// <summary>
    /// Whether a transport section must carry a checksum. True: IPv6 has no
    /// header checksum of its own, so RFC 8200 section 8.1 makes the
    /// upper-layer checksum mandatory, UDP included.
    /// </summary>
    internal override bool TransportChecksumRequired => true;

    /// <summary>
    /// Computes a transport checksum with the IPv6 pseudo-header of RFC 8200
    /// section 8.1: the source and destination addresses, the upper-layer
    /// length as 32 bits, three zero bytes and the next-header value.
    /// </summary>
    /// <param name="protocol">The next-header value to put in the pseudo-header.</param>
    /// <param name="length">The upper-layer length: the whole IPv6 payload.</param>
    internal override ushort ComputeTransportChecksum(byte protocol, ushort length)
    {
        // The 32-bit length folds to its low word for any payload a frame can
        // hold, and the three zero bytes pair the next-header value into one
        // word, so both add as-is.
        uint sum = SumWords(RawData, SourceOffset, 32);
        sum += length;
        sum += protocol;
        sum += SumWords(RawData, PayloadOffset, length);

        return (ushort)~Fold(sum);
    }

    /// <inheritdoc/>
    internal override bool Enqueue() => OutgoingBuffer.AddPacket(this);

    /// <summary>
    /// The length of the payload following the fixed header, in bytes.
    /// </summary>
    public ushort PayloadLength { get; private set; }

    /// <summary>
    /// The protocol of the payload (58 ICMPv6).
    /// </summary>
    public byte NextHeader { get; private set; }

    /// <summary>
    /// The hop limit. Neighbor Discovery messages carry 255.
    /// </summary>
    public byte HopLimit { get; private set; }

    /// <summary>
    /// The source address.
    /// </summary>
    public override Address6 SourceIP => _sourceIP;

    /// <summary>
    /// The destination address.
    /// </summary>
    public override Address6 DestinationIP => _destinationIP;

    /// <summary>
    /// The offset of the payload from the start of the frame. Extension
    /// headers are not parsed, so this is always
    /// <see cref="PayloadOffset"/>.
    /// </summary>
    public override ushort DataOffset => PayloadOffset;

    /// <summary>
    /// The length of the payload in bytes, the same as
    /// <see cref="PayloadLength"/>. IPv6 states the payload length directly,
    /// where IPv4 states a total length the header has to be subtracted from.
    /// </summary>
    public override ushort DataLength => PayloadLength;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"IPv6 Packet Src={SourceIP}, Dest={DestinationIP}, NextHeader={NextHeader}, HopLimit={HopLimit}, PayloadLen={PayloadLength}";
    }
}
