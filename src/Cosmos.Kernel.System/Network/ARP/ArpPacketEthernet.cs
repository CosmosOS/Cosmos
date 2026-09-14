using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv4;

namespace Cosmos.Kernel.System.Network.ARP;

/// <summary>
/// Represents an ARP packet for IPv4 over Ethernet: hardware type 1, protocol type 0x0800,
/// with 6-byte hardware and 4-byte protocol addresses. The address properties are snapshots
/// parsed from <see cref="EthernetPacket.RawData"/> once, at construction, and are never
/// re-read afterwards.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public abstract class ArpPacketEthernet : ArpPacket
{
    /// <summary>
    /// The sender MAC address.
    /// </summary>
    private protected MACAddress _senderMac = null!;

    /// <summary>
    /// The target MAC address.
    /// </summary>
    private protected MACAddress _targetMac = null!;

    /// <summary>
    /// The sender IP address.
    /// </summary>
    private protected Address _senderIP = null!;

    /// <summary>
    /// The target IP address.
    /// </summary>
    private protected Address _targetIP = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArpPacketEthernet"/> class from a received
    /// frame. The array is aliased without copying: <see cref="EthernetPacket.RawData"/> refers
    /// to <paramref name="rawData"/> itself, and the address fields are parsed from it once,
    /// at construction.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame, starting at the destination MAC address.</param>
    public ArpPacketEthernet(byte[] rawData)
        : base(rawData)
    { }

    /// <summary>
    /// Parses the sender and target hardware and protocol addresses (SHA, SPA, THA, TPA) from
    /// <see cref="EthernetPacket.RawData"/> into the protected fields. Called once during
    /// construction; the parsed values are never refreshed afterwards.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _senderMac = new MACAddress(RawData, 22);
        _senderIP = new Address4(RawData, 28);
        _targetMac = new MACAddress(RawData, 32);
        _targetIP = new Address4(RawData, 38);
    }

    /// <summary>
    /// Initializes a new IPv4-over-Ethernet ARP packet for sending. Allocates a frame of
    /// <paramref name="packetSize"/> bytes, writes the Ethernet header (destination
    /// <paramref name="targetMac"/>, source <paramref name="senderMac"/>, EtherType 0x0806),
    /// the fixed ARP header (hardware type 1, protocol type 0x0800, address lengths 6 and 4,
    /// <paramref name="operation"/>), and the four address fields, then parses them back into
    /// the properties.
    /// </summary>
    /// <param name="operation">Operation code (OPER); 1 for a request, 2 for a reply.</param>
    /// <param name="senderMac">Sender hardware address (SHA); also the Ethernet source address.</param>
    /// <param name="senderIP">Sender protocol address (SPA).</param>
    /// <param name="targetMac">Destination MAC address of the Ethernet frame.</param>
    /// <param name="targetIP">Target protocol address (TPA).</param>
    /// <param name="packetSize">Total frame size in bytes.</param>
    /// <param name="arpTargetMac">Target hardware address (THA), the value written into the ARP
    /// body at offset 32; it can differ from the Ethernet destination, as in a broadcast request.</param>
    private protected ArpPacketEthernet(ushort operation, MACAddress senderMac, Address senderIP,
        MACAddress targetMac, Address targetIP, int packetSize, MACAddress arpTargetMac)
        : base(targetMac, senderMac, 1, 0x0800, 6, 4, operation, packetSize)
    {
        for (int i = 0; i < 6; i++)
        {
            RawData[22 + i] = senderMac._bytes[i];
            RawData[32 + i] = arpTargetMac._bytes[i];
        }
        for (int i = 0; i < 4; i++)
        {
            RawData[28 + i] = senderIP.Parts[i];
            RawData[38 + i] = targetIP.Parts[i];
        }

        InitializeFields();
    }

    /// <summary>
    /// Gets the sender hardware address (SHA). This is a snapshot parsed from
    /// <see cref="EthernetPacket.RawData"/> at construction.
    /// </summary>
    public MACAddress SenderMac => _senderMac;

    /// <summary>
    /// Gets the target hardware address (THA), read from the ARP body, not from the Ethernet
    /// header. This is a snapshot parsed from <see cref="EthernetPacket.RawData"/> at construction.
    /// </summary>
    public MACAddress TargetMac => _targetMac;

    /// <summary>
    /// Gets the sender protocol address (SPA). This is a snapshot parsed from
    /// <see cref="EthernetPacket.RawData"/> at construction.
    /// </summary>
    public Address SenderIP => _senderIP;

    /// <summary>
    /// Gets the target protocol address (TPA). This is a snapshot parsed from
    /// <see cref="EthernetPacket.RawData"/> at construction.
    /// </summary>
    public Address TargetIP => _targetIP;

    /// <summary>
    /// Returns a string listing the sender and target MAC addresses, sender and target IP
    /// addresses, and the operation code.
    /// </summary>
    /// <returns>A string representation of the packet.</returns>
    public override string ToString()
    {
        return $"IPv4 Ethernet ARP Packet SenderMac={_senderMac}, TargetMac={_targetMac}, SenderIP={_senderIP}, TargetIP={_targetIP}, Operation={_opCode}";
    }
}

/// <summary>
/// Represents an ARP reply packet (operation code 2) for IPv4 over Ethernet.
/// </summary>
/// <remarks>
/// See also: <seealso cref="ArpPacketEthernet"/>.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public class ArpReplyEthernet : ArpPacketEthernet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArpReplyEthernet"/> class from a received
    /// frame. The array is aliased without copying, and the header fields are parsed from it
    /// once, at construction.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame, starting at the destination MAC address.</param>
    public ArpReplyEthernet(byte[] rawData)
        : base(rawData)
    { }

    /// <summary>
    /// Initializes a new 42-byte ARP reply for sending. The frame is sent unicast to
    /// <paramref name="targetMac"/>, which is written both as the Ethernet destination and as
    /// the ARP target hardware address (THA), so the reply carries the requester's MAC in both
    /// places.
    /// </summary>
    /// <param name="ourMac">Our MAC address: the sender hardware address (SHA) and the Ethernet
    /// source address.</param>
    /// <param name="ourIP">Our IP address: the sender protocol address (SPA).</param>
    /// <param name="targetMac">The requester's MAC address: the Ethernet destination and the ARP
    /// target hardware address (THA).</param>
    /// <param name="targetIP">The requester's IP address: the target protocol address (TPA).</param>
    public ArpReplyEthernet(MACAddress ourMac, Address ourIP, MACAddress targetMac, Address targetIP)
        : base(2, ourMac, ourIP, targetMac, targetIP, 42, targetMac)
    { }

    /// <summary>
    /// Returns a string listing the source and destination MAC addresses and the sender and
    /// target IP addresses.
    /// </summary>
    /// <returns>A string representation of the packet.</returns>
    public override string ToString()
    {
        return $"ARP Reply Src={_srcMAC}, Dest={_destMAC}, Sender={_senderIP}, Target={_targetIP}";
    }
}

/// <summary>
/// Represents an ARP request packet (operation code 1) for IPv4 over Ethernet.
/// </summary>
/// <remarks>
/// See also: <seealso cref="ArpPacketEthernet"/>.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public class ArpRequestEthernet : ArpPacketEthernet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArpRequestEthernet"/> class from a received
    /// frame. The array is aliased without copying, and the header fields are parsed from it
    /// once, at construction.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame, starting at the destination MAC address.</param>
    public ArpRequestEthernet(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Initializes a new 42-byte ARP request for sending. Callers normally pass
    /// <see cref="MACAddress.Broadcast"/> as <paramref name="targetMac"/> (the Ethernet
    /// destination) and <see cref="MACAddress.None"/> as <paramref name="arpTargetMac"/>: the
    /// target hardware address field of a request is zero because it is the value being asked for.
    /// </summary>
    /// <param name="ourMac">Our MAC address: the sender hardware address (SHA) and the Ethernet
    /// source address.</param>
    /// <param name="ourIP">Our IP address: the sender protocol address (SPA).</param>
    /// <param name="targetMac">Destination MAC address of the Ethernet frame, normally
    /// <see cref="MACAddress.Broadcast"/>.</param>
    /// <param name="targetIP">The IP address being resolved: the target protocol address (TPA).</param>
    /// <param name="arpTargetMac">Target hardware address (THA) written into the ARP body,
    /// normally <see cref="MACAddress.None"/>.</param>
    public ArpRequestEthernet(MACAddress ourMac, Address ourIP, MACAddress targetMac, Address targetIP, MACAddress arpTargetMac)
        : base(1, ourMac, ourIP, targetMac, targetIP, 42, arpTargetMac)
    { }

    /// <summary>
    /// Returns a string listing the source and destination MAC addresses and the sender and
    /// target IP addresses.
    /// </summary>
    /// <returns>A string representation of the packet.</returns>
    public override string ToString()
    {
        return $"ARP Request Src={_srcMAC}, Dest={_destMAC}, Sender={_senderIP}, Target={_targetIP}";
    }
}
