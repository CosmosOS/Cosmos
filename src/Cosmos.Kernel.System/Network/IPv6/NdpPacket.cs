// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network.IPv6;

/// <summary>
/// A Neighbor Discovery message (RFC 4861): the ICMPv6 header, whose
/// message-specific word carries the flags, a 16-byte target address and
/// options. The messages built here carry one link-layer address option.
/// </summary>
internal abstract class NdpPacket : Icmpv6Packet
{
    /// <summary>
    /// Hop limit every Neighbor Discovery message carries. A receiver drops
    /// any other value: it proves the message did not cross a router.
    /// </summary>
    internal const byte NdpHopLimit = 255;

    /// <summary>
    /// Smallest payload a solicitation or advertisement can have: the
    /// ICMPv6 header and the target address.
    /// </summary>
    internal const int MinimumLength = Icmpv6HeaderLength + 16;

    /// <summary>Option type of a source link-layer address.</summary>
    private protected const byte OptionSourceLinkLayer = 1;

    /// <summary>Option type of a target link-layer address.</summary>
    private protected const byte OptionTargetLinkLayer = 2;

    private const int TargetOffset = PayloadOffset + Icmpv6HeaderLength;
    private const int OptionsOffset = TargetOffset + 16;
    private const int LinkLayerOptionLength = 8;

    /// <summary>Payload of a message built here: the header, the target and one link-layer option.</summary>
    private const ushort MessageLength = MinimumLength + LinkLayerOptionLength;

    /// <summary>Number of solicitations for one of the stack's addresses answered with an advertisement.</summary>
    private protected static int s_solicitationsAnswered;

    /// <summary>Number of advertisements recorded in the neighbor cache.</summary>
    private protected static int s_advertisementsReceived;

    /// <summary>Parsed target address backing <see cref="Target"/>.</summary>
    private protected Address6 _target = null!;

    /// <summary>
    /// Number of solicitations for one of the stack's addresses answered
    /// with an advertisement.
    /// </summary>
    internal static int SolicitationsAnswered => s_solicitationsAnswered;

    /// <summary>
    /// Number of advertisements recorded in the neighbor cache.
    /// </summary>
    internal static int AdvertisementsReceived => s_advertisementsReceived;

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied.
    /// </summary>
    /// <param name="rawData">The raw data of the frame.</param>
    private protected NdpPacket(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Builds a message with hop limit 255: the flags word, the target and
    /// one link-layer address option holding <paramref name="linkLayer"/>.
    /// </summary>
    /// <param name="sourceMac">Source MAC address.</param>
    /// <param name="destinationMac">Destination MAC address.</param>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address.</param>
    /// <param name="type">ICMPv6 type.</param>
    /// <param name="flags">The first byte of the message-specific word; the other three are reserved and zero.</param>
    /// <param name="target">The target address.</param>
    /// <param name="optionType">The link-layer option type.</param>
    /// <param name="linkLayer">The link-layer address the option carries.</param>
    private protected NdpPacket(MACAddress sourceMac, MACAddress destinationMac, Address6 source, Address6 destination,
        byte type, byte flags, Address6 target, byte optionType, MACAddress linkLayer)
        : base(sourceMac, destinationMac, source, destination, NdpHopLimit, type, 0, MessageLength)
    {
        RawData[PayloadOffset + 4] = flags;
        RawData[PayloadOffset + 5] = 0;
        RawData[PayloadOffset + 6] = 0;
        RawData[PayloadOffset + 7] = 0;
        target.ToBytes().CopyTo(RawData.AsSpan(TargetOffset, 16));
        RawData[OptionsOffset] = optionType;
        RawData[OptionsOffset + 1] = LinkLayerOptionLength / 8;
        linkLayer._bytes.AsSpan().CopyTo(RawData.AsSpan(OptionsOffset + 2, 6));

        WriteChecksum();
    }

    /// <summary>
    /// Parses the target address snapshot, in addition to the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _target = new Address6(RawData, TargetOffset);
    }

    /// <summary>
    /// The link-layer address carried by the first option of
    /// <paramref name="optionType"/>, or null when the message has none.
    /// </summary>
    private protected MACAddress? FindLinkLayerOption(byte optionType)
    {
        int end = Math.Min(PayloadOffset + PayloadLength, RawData.Length);
        int offset = OptionsOffset;
        while (offset + 2 <= end)
        {
            int length = RawData[offset + 1] * 8;
            if (length == 0)
            {
                break;
            }

            if (RawData[offset] == optionType && length == LinkLayerOptionLength && offset + length <= end)
            {
                return new MACAddress(RawData, offset + 2);
            }

            offset += length;
        }

        return null;
    }

    /// <summary>
    /// The target address, a snapshot parsed at construction time.
    /// </summary>
    public Address6 Target => _target;
}

/// <summary>
/// A Neighbor Solicitation (type 135): asks the node owning
/// <see cref="NdpPacket.Target"/> for its link-layer address.
/// </summary>
internal sealed class NeighborSolicitation : NdpPacket
{
    private MACAddress? _sourceLinkLayerAddress;

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied.
    /// </summary>
    /// <param name="rawData">The raw data of the frame.</param>
    public NeighborSolicitation(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Builds a solicitation for <paramref name="target"/>, sent to its
    /// solicited-node multicast group with a source link-layer address
    /// option, so the answering node can record the sender without a
    /// solicitation of its own.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="target">The address to resolve.</param>
    /// <param name="sourceMac">The sending device's MAC address.</param>
    internal NeighborSolicitation(Address6 source, Address6 target, MACAddress sourceMac)
        : this(source, target, sourceMac, target.ToSolicitedNodeMulticast())
    {
    }

    private NeighborSolicitation(Address6 source, Address6 target, MACAddress sourceMac, Address6 group)
        : base(sourceMac, MulticastMac(group), source, group, TypeNeighborSolicitation, 0, target, OptionSourceLinkLayer, sourceMac)
    {
    }

    /// <summary>
    /// Answers a solicitation for one of the stack's addresses with a
    /// solicited advertisement, recording the sender's link-layer address
    /// first. A solicitation from the unspecified address is a duplicate
    /// address detection probe and is ignored: the stack does not defend its
    /// addresses yet.
    /// </summary>
    /// <param name="packetData">The raw data of the frame.</param>
    internal static void Handle(byte[] packetData)
    {
        NeighborSolicitation solicitation = new(packetData);

        if (solicitation.HopLimit != NdpHopLimit || solicitation.SourceIP.IsZero)
        {
            return;
        }

        if (!NetworkStack.AddressMap.TryGetValue(solicitation.Target, out INetworkDevice? nic))
        {
            return;
        }

        Serial.WriteString("[NDP] Solicitation for ");
        Serial.WriteString(solicitation.Target.ToString());
        Serial.WriteString(" from ");
        Serial.WriteString(solicitation.SourceIP.ToString());
        Serial.WriteString("\n");

        MACAddress requester = solicitation.SourceLinkLayerAddress ?? solicitation.SourceMac;
        NeighborCache.Update(solicitation.SourceIP, requester);

        NeighborAdvertisement advertisement = new(solicitation.Target, solicitation.SourceIP, nic.MacAddress, requester, solicitation.Target);
        nic.Send(advertisement.RawData, advertisement.RawData.Length);
        s_solicitationsAnswered++;
    }

    /// <summary>
    /// Parses the source link-layer address option, in addition to the base
    /// fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _sourceLinkLayerAddress = FindLinkLayerOption(OptionSourceLinkLayer);
    }

    /// <summary>
    /// The sender's link-layer address from the source link-layer option, or
    /// null when the solicitation carries none.
    /// </summary>
    public MACAddress? SourceLinkLayerAddress => _sourceLinkLayerAddress;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Neighbor Solicitation Src={SourceIP}, Dest={DestinationIP}, Target={_target}";
    }
}

/// <summary>
/// A Neighbor Advertisement (type 136): the answer to a solicitation,
/// carrying the link-layer address of <see cref="NdpPacket.Target"/>.
/// </summary>
internal sealed class NeighborAdvertisement : NdpPacket
{
    private const byte FlagSolicited = 0x40;
    private const byte FlagOverride = 0x20;

    private MACAddress? _targetLinkLayerAddress;

    /// <summary>
    /// Initializes a new instance over existing frame bytes. The array is
    /// aliased, not copied.
    /// </summary>
    /// <param name="rawData">The raw data of the frame.</param>
    public NeighborAdvertisement(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Builds a solicited advertisement for <paramref name="target"/> with
    /// the override flag set and a target link-layer address option holding
    /// <paramref name="sourceMac"/>.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="destination">Destination address: the solicitation's source.</param>
    /// <param name="sourceMac">The advertising device's MAC address.</param>
    /// <param name="destinationMac">The requester's MAC address.</param>
    /// <param name="target">The address being advertised.</param>
    internal NeighborAdvertisement(Address6 source, Address6 destination, MACAddress sourceMac, MACAddress destinationMac, Address6 target)
        : base(sourceMac, destinationMac, source, destination, TypeNeighborAdvertisement, FlagSolicited | FlagOverride, target, OptionTargetLinkLayer, sourceMac)
    {
    }

    /// <summary>
    /// Records the advertised link-layer address in the neighbor cache, which
    /// releases any packet waiting on it in the outgoing queue.
    /// </summary>
    /// <param name="packetData">The raw data of the frame.</param>
    internal static void Handle(byte[] packetData)
    {
        NeighborAdvertisement advertisement = new(packetData);

        if (advertisement.HopLimit != NdpHopLimit)
        {
            return;
        }

        Serial.WriteString("[NDP] Advertisement for ");
        Serial.WriteString(advertisement.Target.ToString());
        Serial.WriteString("\n");

        NeighborCache.Update(advertisement.Target, advertisement.TargetLinkLayerAddress ?? advertisement.SourceMac);
        s_advertisementsReceived++;
    }

    /// <summary>
    /// Parses the target link-layer address option, in addition to the base
    /// fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _targetLinkLayerAddress = FindLinkLayerOption(OptionTargetLinkLayer);
    }

    /// <summary>
    /// Whether the advertisement answers a solicitation (the S flag).
    /// </summary>
    public bool IsSolicited => (RawData[PayloadOffset + 4] & FlagSolicited) != 0;

    /// <summary>
    /// The target's link-layer address from the target link-layer option, or
    /// null when the advertisement carries none.
    /// </summary>
    public MACAddress? TargetLinkLayerAddress => _targetLinkLayerAddress;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Neighbor Advertisement Src={SourceIP}, Dest={DestinationIP}, Target={_target}";
    }
}
