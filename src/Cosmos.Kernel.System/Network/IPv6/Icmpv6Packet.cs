// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network.IPv6;

/// <summary>
/// An ICMPv6 packet carried in an <see cref="IPv6Packet"/> (next header 58).
/// Header properties are snapshots parsed from <see cref="EthernetPacket.RawData"/>
/// at construction time; the checksum is written by the build constructors
/// and never recomputed afterwards.
/// </summary>
internal class Icmpv6Packet : IPv6Packet
{
    /// <summary>ICMPv6 type of an echo request.</summary>
    internal const byte TypeEchoRequest = 128;

    /// <summary>ICMPv6 type of an echo reply.</summary>
    internal const byte TypeEchoReply = 129;

    /// <summary>ICMPv6 type of a Neighbor Solicitation.</summary>
    internal const byte TypeNeighborSolicitation = 135;

    /// <summary>ICMPv6 type of a Neighbor Advertisement.</summary>
    internal const byte TypeNeighborAdvertisement = 136;

    /// <summary>
    /// Length of the ICMPv6 header: type, code, checksum and the
    /// message-specific word.
    /// </summary>
    internal const int Icmpv6HeaderLength = 8;

    /// <summary>Hop limit of an echo request or reply.</summary>
    private protected const byte EchoHopLimit = 64;

    private static int s_echoRequestsReplied;
    private static byte[]? s_lastEchoRequestData;

    /// <summary>Parsed ICMPv6 type backing <see cref="IcmpType"/>.</summary>
    private protected byte _icmpType;

    /// <summary>Parsed ICMPv6 code backing <see cref="IcmpCode"/>.</summary>
    private protected byte _icmpCode;

    /// <summary>Parsed or computed checksum backing <see cref="IcmpChecksum"/>.</summary>
    private protected ushort _icmpChecksum;

    /// <summary>
    /// Number of echo requests answered with an echo reply.
    /// </summary>
    internal static int EchoRequestsReplied => s_echoRequestsReplied;

    /// <summary>
    /// ICMPv6 payload of the most recently answered echo request.
    /// </summary>
    internal static byte[]? LastEchoRequestData => s_lastEchoRequestData;

    /// <summary>
    /// Handles an ICMPv6 packet whose destination the stack owns. A packet
    /// whose checksum does not verify is dropped.
    /// </summary>
    /// <param name="packetData">The raw data of the frame.</param>
    internal static void Icmpv6Handler(byte[] packetData)
    {
        Icmpv6Packet packet = new(packetData);

        if (!packet.VerifyChecksum())
        {
            Serial.WriteString("[ICMPv6] Bad checksum, dropping\n");
            return;
        }

        switch (packet.IcmpType)
        {
            case TypeEchoRequest:
                HandleEchoRequest(packetData);
                break;
            case TypeEchoReply:
                Serial.WriteString("[ICMPv6] Received echo reply from ");
                Serial.WriteString(packet.SourceIP.ToString());
                Serial.WriteString("\n");

                Icmpv6Client.GetClient(packet.SourceIP)?.ReceiveData(new Icmpv6EchoReply(packetData));
                break;
            case TypeNeighborSolicitation when packet.PayloadLength >= NdpPacket.MinimumLength:
                NeighborSolicitation.Handle(packetData);
                break;
            case TypeNeighborAdvertisement when packet.PayloadLength >= NdpPacket.MinimumLength:
                NeighborAdvertisement.Handle(packetData);
                break;
        }
    }

    /// <summary>
    /// Answers an echo request sent to one of the stack's unicast addresses.
    /// The reply goes straight back to the MAC the request came from: this
    /// runs in the receive path, where waiting on Neighbor Discovery is not
    /// an option. A multicast destination names no address to answer from,
    /// so such a request is ignored.
    /// </summary>
    private static void HandleEchoRequest(byte[] packetData)
    {
        Icmpv6EchoRequest request = new(packetData);

        if (!NetworkStack.AddressMap.TryGetValue(request.DestinationIP, out INetworkDevice? nic))
        {
            return;
        }

        Icmpv6EchoReply reply = new(request, nic.MacAddress);

        Serial.WriteString("[ICMPv6] Sending echo reply to ");
        Serial.WriteString(reply.DestinationIP.ToString());
        Serial.WriteString("\n");

        nic.Send(reply.RawData, reply.RawData.Length);

        s_lastEchoRequestData = request.GetIcmpData();
        s_echoRequestsReplied++;
    }

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied: the caller must not reuse the buffer while the
    /// packet is alive.
    /// </summary>
    /// <param name="rawData">The raw data of the frame.</param>
    public Icmpv6Packet(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Initializes a new instance, resolving the MAC addresses as
    /// <see cref="IPv6Packet"/> does, and writes the type and code. The
    /// derived constructor writes the rest of the section and then calls
    /// <see cref="WriteChecksum"/>.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    /// <param name="hopLimit">Hop limit.</param>
    /// <param name="type">ICMPv6 type.</param>
    /// <param name="code">ICMPv6 code.</param>
    /// <param name="icmpLength">Length of the ICMPv6 header plus body: the whole IPv6 payload.</param>
    private protected Icmpv6Packet(Address6 source, Address6 destination, byte hopLimit, byte type, byte code, ushort icmpLength)
        : base(icmpLength, ProtocolIcmpv6, hopLimit, source, destination)
    {
        WriteTypeAndCode(type, code);
    }

    /// <summary>
    /// Initializes a new instance with explicit MAC addresses and writes the
    /// type and code. The derived constructor writes the rest of the section
    /// and then calls <see cref="WriteChecksum"/>.
    /// </summary>
    /// <param name="sourceMac">Source MAC address.</param>
    /// <param name="destinationMac">Destination MAC address.</param>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    /// <param name="hopLimit">Hop limit.</param>
    /// <param name="type">ICMPv6 type.</param>
    /// <param name="code">ICMPv6 code.</param>
    /// <param name="icmpLength">Length of the ICMPv6 header plus body: the whole IPv6 payload.</param>
    private protected Icmpv6Packet(MACAddress sourceMac, MACAddress destinationMac, Address6 source, Address6 destination, byte hopLimit, byte type, byte code, ushort icmpLength)
        : base(sourceMac, destinationMac, icmpLength, ProtocolIcmpv6, hopLimit, source, destination)
    {
        WriteTypeAndCode(type, code);
    }

    private void WriteTypeAndCode(byte type, byte code)
    {
        RawData[PayloadOffset] = type;
        RawData[PayloadOffset + 1] = code;
        RawData[PayloadOffset + 2] = 0;
        RawData[PayloadOffset + 3] = 0;
    }

    /// <summary>
    /// Parses the type, code and checksum snapshots from
    /// <see cref="EthernetPacket.RawData"/>, in addition to the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _icmpType = RawData[PayloadOffset];
        _icmpCode = RawData[PayloadOffset + 1];
        _icmpChecksum = (ushort)((RawData[PayloadOffset + 2] << 8) | RawData[PayloadOffset + 3]);
    }

    /// <summary>
    /// Computes the checksum over the section as it is now, stores it, and
    /// re-parses the header snapshots. A derived constructor calls this once
    /// its body is written; nothing recomputes it afterwards.
    /// </summary>
    private protected void WriteChecksum()
    {
        RawData[PayloadOffset + 2] = 0;
        RawData[PayloadOffset + 3] = 0;
        ushort checksum = CalcUpperLayerChecksum(PayloadLength);
        RawData[PayloadOffset + 2] = (byte)(checksum >> 8);
        RawData[PayloadOffset + 3] = (byte)checksum;
        InitializeFields();
    }

    /// <summary>
    /// The ICMPv6 type, a snapshot parsed at construction time.
    /// </summary>
    public byte IcmpType => _icmpType;

    /// <summary>
    /// The ICMPv6 code, a snapshot parsed at construction time.
    /// </summary>
    public byte IcmpCode => _icmpCode;

    /// <summary>
    /// The checksum stored in the section, a snapshot taken at construction time.
    /// </summary>
    public ushort IcmpChecksum => _icmpChecksum;

    /// <summary>
    /// Checks the stored checksum against the section and the pseudo-header,
    /// as a receiver does. Sums the whole section on every call.
    /// </summary>
    /// <returns>True when the section is intact.</returns>
    public bool VerifyChecksum()
    {
        return CalcUpperLayerChecksum(PayloadLength) == 0;
    }

    /// <summary>
    /// The length in bytes of the body after the 8-byte ICMPv6 header.
    /// </summary>
    public ushort IcmpDataLength => (ushort)(PayloadLength - Icmpv6HeaderLength);

    /// <summary>
    /// Returns a fresh copy of the body after the 8-byte ICMPv6 header.
    /// Mutating the returned array does not affect the packet.
    /// </summary>
    /// <returns>A new array holding the body bytes.</returns>
    public byte[] GetIcmpData()
    {
        byte[] data = new byte[IcmpDataLength];
        RawData.AsSpan(PayloadOffset + Icmpv6HeaderLength, IcmpDataLength).CopyTo(data);
        return data;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ICMPv6 Packet Src={SourceIP}, Dest={DestinationIP}, Type={_icmpType}, Code={_icmpCode}";
    }
}

/// <summary>
/// An ICMPv6 echo request (type 128). The identifier and sequence properties
/// are snapshots parsed at construction time.
/// </summary>
internal sealed class Icmpv6EchoRequest : Icmpv6Packet
{
    /// <summary>
    /// Body length of a request built here, the same as the IPv4 echo request.
    /// </summary>
    internal const int EchoDataLength = 32;

    private ushort _icmpId;
    private ushort _icmpSequence;

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied.
    /// </summary>
    /// <param name="rawData">The raw data of the frame.</param>
    public Icmpv6EchoRequest(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Builds a request with a 32-byte body whose bytes hold their own offset
    /// in the ICMPv6 section, as the IPv4 request does.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    /// <param name="id">Echo identifier.</param>
    /// <param name="sequence">Echo sequence number.</param>
    internal Icmpv6EchoRequest(Address6 source, Address6 destination, ushort id, ushort sequence)
        : base(source, destination, EchoHopLimit, TypeEchoRequest, 0, Icmpv6HeaderLength + EchoDataLength)
    {
        RawData[PayloadOffset + 4] = (byte)(id >> 8);
        RawData[PayloadOffset + 5] = (byte)id;
        RawData[PayloadOffset + 6] = (byte)(sequence >> 8);
        RawData[PayloadOffset + 7] = (byte)sequence;
        for (int b = Icmpv6HeaderLength; b < Icmpv6HeaderLength + EchoDataLength; b++)
        {
            RawData[PayloadOffset + b] = (byte)b;
        }

        WriteChecksum();
    }

    /// <summary>
    /// Parses the identifier and sequence number snapshots, in addition to
    /// the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _icmpId = (ushort)((RawData[PayloadOffset + 4] << 8) | RawData[PayloadOffset + 5]);
        _icmpSequence = (ushort)((RawData[PayloadOffset + 6] << 8) | RawData[PayloadOffset + 7]);
    }

    /// <summary>
    /// The echo identifier, a snapshot parsed at construction time.
    /// </summary>
    public ushort IcmpId => _icmpId;

    /// <summary>
    /// The echo sequence number, a snapshot parsed at construction time.
    /// </summary>
    public ushort IcmpSequence => _icmpSequence;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ICMPv6 Echo Request Src={SourceIP}, Dest={DestinationIP}, ID={_icmpId}, Sequence={_icmpSequence}";
    }
}

/// <summary>
/// An ICMPv6 echo reply (type 129). The identifier and sequence properties
/// are snapshots parsed at construction time.
/// </summary>
internal sealed class Icmpv6EchoReply : Icmpv6Packet
{
    private ushort _icmpId;
    private ushort _icmpSequence;

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied.
    /// </summary>
    /// <param name="rawData">The raw data of the frame.</param>
    public Icmpv6EchoReply(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Builds the reply to <paramref name="request"/>: source and destination
    /// swapped, identifier, sequence number and body copied, addressed to the
    /// MAC the request came from.
    /// </summary>
    /// <param name="request">The echo request to answer.</param>
    /// <param name="sourceMac">The answering device's MAC address.</param>
    internal Icmpv6EchoReply(Icmpv6EchoRequest request, MACAddress sourceMac)
        : base(sourceMac, request.SourceMac, request.DestinationIP, request.SourceIP, EchoHopLimit, TypeEchoReply, 0, request.PayloadLength)
    {
        request.RawData.AsSpan(PayloadOffset + 4, request.PayloadLength - 4).CopyTo(RawData.AsSpan(PayloadOffset + 4));
        WriteChecksum();
    }

    /// <summary>
    /// Parses the identifier and sequence number snapshots, in addition to
    /// the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _icmpId = (ushort)((RawData[PayloadOffset + 4] << 8) | RawData[PayloadOffset + 5]);
        _icmpSequence = (ushort)((RawData[PayloadOffset + 6] << 8) | RawData[PayloadOffset + 7]);
    }

    /// <summary>
    /// The echo identifier, a snapshot parsed at construction time.
    /// </summary>
    public ushort IcmpId => _icmpId;

    /// <summary>
    /// The echo sequence number, a snapshot parsed at construction time.
    /// </summary>
    public ushort IcmpSequence => _icmpSequence;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ICMPv6 Echo Reply Src={SourceIP}, Dest={DestinationIP}, ID={_icmpId}, Sequence={_icmpSequence}";
    }
}
