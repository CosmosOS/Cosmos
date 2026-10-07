// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices;

namespace Cosmos.Kernel.System.Network.Protocols.Arp;

/// <summary>
/// Represents an ARP (Address Resolution Protocol) frame carried over Ethernet with EtherType 0x0806.
/// The header properties are snapshots parsed from <see cref="EthernetPacket.RawData"/> once, at
/// construction, and are never re-read afterwards.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class ArpPacket : EthernetPacket
{
    /// <summary>
    /// The hardware type field (HTYPE) parsed from the frame at construction; 1 means Ethernet.
    /// </summary>
    private protected ushort _hardwareType;

    /// <summary>
    /// The protocol type field (PTYPE) parsed from the frame at construction; 0x0800 means IPv4.
    /// </summary>
    private protected ushort _protocolType;

    /// <summary>
    /// The hardware address length field (HLEN) parsed from the frame at construction; 6 for Ethernet.
    /// </summary>
    private protected byte _hardwareAddrLength;

    /// <summary>
    /// The protocol address length field (PLEN) parsed from the frame at construction; 4 for IPv4.
    /// </summary>
    private protected byte _protocolAddrLength;

    /// <summary>
    /// The operation code field (OPER) parsed from the frame at construction; 1 is a request, 2 is a reply.
    /// </summary>
    private protected ushort _opCode;

    /// <summary>
    /// Gets the operation code (OPER); 1 is a request, 2 is a reply. This is a snapshot parsed
    /// from <see cref="EthernetPacket.RawData"/> at construction.
    /// </summary>
    public ushort Operation => _opCode;

    /// <summary>
    /// Gets the hardware type (HTYPE); 1 means Ethernet. This is a snapshot parsed from
    /// <see cref="EthernetPacket.RawData"/> at construction.
    /// </summary>
    public ushort HardwareType => _hardwareType;

    /// <summary>
    /// Gets the protocol type (PTYPE); 0x0800 means IPv4. This is a snapshot parsed from
    /// <see cref="EthernetPacket.RawData"/> at construction.
    /// </summary>
    public ushort ProtocolType => _protocolType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArpPacket"/> class from a received frame.
    /// The array is aliased without copying: <see cref="EthernetPacket.RawData"/> refers to
    /// <paramref name="rawData"/> itself, and the ARP header fields are parsed from it once,
    /// at construction.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame, starting at the destination MAC address.</param>
    public ArpPacket(byte[] rawData)
        : base(rawData)
    { }

    /// <summary>
    /// Initializes a new ARP packet for sending. Allocates a frame of <paramref name="packetSize"/>
    /// bytes, writes the Ethernet header with EtherType 0x0806 and the ARP header fields into it,
    /// then parses the header back into the properties. Nothing is recomputed after construction.
    /// </summary>
    /// <param name="dest">Destination MAC address of the Ethernet frame.</param>
    /// <param name="src">Source MAC address of the Ethernet frame.</param>
    /// <param name="hwType">Hardware type (HTYPE); 1 for Ethernet.</param>
    /// <param name="protoType">Protocol type (PTYPE); 0x0800 for IPv4.</param>
    /// <param name="hwLen">Hardware address length in bytes (HLEN); 6 for Ethernet.</param>
    /// <param name="protoLen">Protocol address length in bytes (PLEN); 4 for IPv4.</param>
    /// <param name="operation">Operation code (OPER); 1 for a request, 2 for a reply.</param>
    /// <param name="packetSize">Total frame size in bytes.</param>
    private protected ArpPacket(MacAddress dest, MacAddress src, ushort hwType, ushort protoType,
        byte hwLen, byte protoLen, ushort operation, int packetSize)
        : base(dest, src, 0x0806, packetSize)
    {
        RawData[14] = (byte)(hwType >> 8);
        RawData[15] = (byte)(hwType >> 0);
        RawData[16] = (byte)(protoType >> 8);
        RawData[17] = (byte)(protoType >> 0);
        RawData[18] = hwLen;
        RawData[19] = protoLen;
        RawData[20] = (byte)(operation >> 8);
        RawData[21] = (byte)(operation >> 0);

        InitializeFields();
    }

    /// <summary>
    /// Handles a received IPv4-over-Ethernet ARP frame: records the sender of a request or a
    /// reply in the <see cref="ArpCache"/>, and answers a request whose target address belongs
    /// to one of the registered devices. Any other hardware or protocol type is ignored.
    /// </summary>
    /// <param name="packetData">The raw Ethernet frame, starting at the destination MAC address.</param>
    internal static void ARPHandler(byte[] packetData)
    {
        ArpPacket arpPacket = new(packetData);

        if (arpPacket.Operation == 0x01)
        {
            // ARP Request
            if (arpPacket.HardwareType == 1 && arpPacket.ProtocolType == 0x0800)
            {
                ArpRequestEthernet arpRequest = new(packetData);

                ArpCache.Update(arpRequest.SenderIP, arpRequest.SenderMac);

                if (NetworkStack.AddressMap.TryGetValue(arpRequest.TargetIP, out INetworkDevice? nic))
                {
                    Serial.WriteString("[ARP] Request received from ");
                    Serial.WriteString(arpRequest.SenderIP.ToString());
                    Serial.WriteString("\n");

                    MacAddress nicMac = new(nic.MacAddress);

                    ArpReplyEthernet reply = new(
                        nicMac,
                        arpRequest.TargetIP,
                        arpRequest.SenderMac,
                        arpRequest.SenderIP
                    );

                    nic.Send(reply.RawData, reply.RawData.Length);
                }
            }
        }
        else if (arpPacket.Operation == 0x02)
        {
            // ARP Reply
            if (arpPacket.HardwareType == 1 && arpPacket.ProtocolType == 0x0800)
            {
                ArpReplyEthernet arpReply = new(packetData);
                Serial.WriteString("[ARP] Reply received from ");
                Serial.WriteString(arpReply.SenderIP.ToString());
                Serial.WriteString("\n");
                ArpCache.Update(arpReply.SenderIP, arpReply.SenderMac);
            }
        }
    }

    /// <summary>
    /// Parses the ARP header fields (hardware type, protocol type, address lengths, operation code)
    /// from <see cref="EthernetPacket.RawData"/> into the protected fields. Called once during
    /// construction; the parsed values are never refreshed afterwards.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _hardwareType = (ushort)((RawData[14] << 8) | RawData[15]);
        _protocolType = (ushort)((RawData[16] << 8) | RawData[17]);
        _hardwareAddrLength = RawData[18];
        _protocolAddrLength = RawData[19];
        _opCode = (ushort)((RawData[20] << 8) | RawData[21]);
    }

    /// <summary>
    /// Returns a string listing the source and destination MAC addresses, hardware type,
    /// protocol type, and operation code.
    /// </summary>
    /// <returns>A string representation of the packet.</returns>
    public override string ToString() =>
        $"ARP Packet Src={_srcMAC}, Dest={_destMAC}, HWType={_hardwareType}, Protocol={_protocolType}, Operation={Operation}";
}
