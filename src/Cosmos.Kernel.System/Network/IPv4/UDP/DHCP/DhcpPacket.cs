/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DHCP Packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network.IPv4.UDP.DHCP;

/// <summary>
/// Represents a single DHCP option parsed from the options section of a <see cref="DhcpPacket"/>.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DhcpOption
{
    /// <summary>
    /// Gets the DHCP option code, for example 1 for subnet mask, 3 for router, 6 for domain name server.
    /// </summary>
    public required byte Type { get; init; }

    /// <summary>
    /// Gets the length in bytes of the option payload, computed from <see cref="Data"/>.
    /// </summary>
    public byte Length => (byte)Data.Length;

    /// <summary>
    /// Gets the raw option payload, without the option code and length bytes.
    /// </summary>
    public required byte[] Data { get; init; }
}

/// <summary>
/// Represents a DHCP packet carried over UDP (BOOTP client port 68, server port 67).
/// Header properties are snapshots parsed from <see cref="EthernetPacket.RawData"/> at construction
/// and are not re-read afterwards; checksums and lengths are computed in the constructors and never
/// recomputed.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DhcpPacket : UdpPacket
{
    // Simple transaction ID generator
    private static int s_idCounter = 1;

    /// <summary>
    /// Handles a single DHCP packet.
    /// </summary>
    internal static void DHCPHandler(byte[] packetData)
    {
        DhcpPacket dhcpPacket = new(packetData);

        UdpClient? receiver = UdpClient.GetClient(dhcpPacket.DestinationPort);
        receiver?.ReceiveData(dhcpPacket);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpPacket"/> class from received data.
    /// The packet aliases <paramref name="rawData"/> without copying, and all header properties
    /// are parsed from it once, during construction.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame bytes, aliased rather than copied.</param>
    public DhcpPacket(byte[] rawData)
        : base(rawData)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpPacket"/> class as a broadcast request,
    /// sent from 0.0.0.0 to 255.255.255.255 (the form used by discover and request packets).
    /// </summary>
    /// <param name="sourceMac">The MAC address of the sending network device.</param>
    /// <param name="dhcpDataSize">The size in bytes of the DHCP options that follow the fixed BOOTP header.</param>
    public DhcpPacket(MACAddress sourceMac, ushort dhcpDataSize)
        : this(Address4.Zero, Address4.Broadcast, sourceMac, dhcpDataSize)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpPacket"/> class addressed from
    /// <paramref name="client"/> to <paramref name="server"/>. Writes the fixed BOOTP header:
    /// op 1 (request), hardware type Ethernet, a fresh transaction identifier, the client address
    /// in ciaddr, the source MAC in chaddr and the DHCP magic cookie. Checksums and lengths are
    /// computed here, in the constructor chain, and never recomputed.
    /// </summary>
    /// <param name="client">The IPv4 source address, also written to the ciaddr field.</param>
    /// <param name="server">The IPv4 destination address.</param>
    /// <param name="sourceMAC">The MAC address of the sending network device.</param>
    /// <param name="dhcpDataSize">The size in bytes of the DHCP options that follow the fixed BOOTP header.</param>
    public DhcpPacket(Address client, Address server, MACAddress sourceMAC, ushort dhcpDataSize)
        : base(client, server, 68, 67, (ushort)(dhcpDataSize + 240), MACAddress.Broadcast)
    {
        RawData[42] = 0x01; // Request
        RawData[43] = 0x01; // ethernet
        RawData[44] = 0x06; // Length mac
        RawData[45] = 0x00; // hops

        int id = s_idCounter++;
        RawData[46] = (byte)((id >> 24) & 0xFF);
        RawData[47] = (byte)((id >> 16) & 0xFF);
        RawData[48] = (byte)((id >> 8) & 0xFF);
        RawData[49] = (byte)((id >> 0) & 0xFF);

        // second elapsed
        RawData[50] = 0x00;
        RawData[51] = 0x00;

        // option bootp
        RawData[52] = 0x00;
        RawData[53] = 0x00;

        // client ip address
        RawData[54] = client.Parts[0];
        RawData[55] = client.Parts[1];
        RawData[56] = client.Parts[2];
        RawData[57] = client.Parts[3];

        for (int i = 0; i < 13; i++)
        {
            RawData[58 + i] = 0x00;
        }

        // Source MAC
        RawData[70] = sourceMAC._bytes[0];
        RawData[71] = sourceMAC._bytes[1];
        RawData[72] = sourceMAC._bytes[2];
        RawData[73] = sourceMAC._bytes[3];
        RawData[74] = sourceMAC._bytes[4];
        RawData[75] = sourceMAC._bytes[5];

        // Fill w/ 0s
        for (int i = 0; i < 202; i++)
        {
            RawData[76 + i] = 0x00;
        }

        // DHCP Magic cookie
        RawData[278] = 0x63;
        RawData[279] = 0x82;
        RawData[280] = 0x53;
        RawData[281] = 0x63;

        InitializeFields();
    }

    /// <summary>
    /// Parses the BOOTP op field, the yiaddr client address and the DHCP options from
    /// <see cref="EthernetPacket.RawData"/>. Called once during construction; the resulting
    /// property values are snapshots that are not updated afterwards.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        Operation = RawData[42];

        if (RawData[58] != 0)
        {
            Client = new Address4(RawData, 58);
        }

        if (RawData[282] != 0)
        {
            Options = [];

            for (int i = 0; i < RawData.Length - 282 && RawData[282 + i] != 0xFF; i += 2) //0xFF is DHCP packet end
            {
                DhcpOption option = new()
                {
                    Type = RawData[282 + i],
                    Data = new byte[RawData[282 + i + 1]]
                };
                for (int j = 0; j < option.Length; j++)
                {
                    option.Data[j] = RawData[282 + i + j + 2];
                }
                Options.Add(option);

                i += option.Length;
            }

            foreach (DhcpOption option in Options)
            {
                if (option.Type == 1) //Mask
                {
                    Subnet = new Address4(option.Data, 0);
                }
                else if (option.Type == 3) //Router
                {
                    Gateway = new Address4(option.Data, 0);
                }
                else if (option.Type == 6) //DNS
                {
                    DNS = new Address4(option.Data, 0);
                }
            }
        }
    }

    /// <summary>
    /// Gets the BOOTP op field at offset 42 of the frame: 1 for a request, 2 for a reply.
    /// This is not the DHCP message type (option 53), which lives in the options section.
    /// A snapshot parsed at construction.
    /// </summary>
    public byte Operation { get; private set; }

    /// <summary>
    /// Gets the client IPv4 address parsed from the BOOTP yiaddr field, or null when the first
    /// byte of yiaddr is zero. A snapshot parsed at construction.
    /// </summary>
    public Address? Client { get; private set; }

    /// <summary>
    /// Gets the DHCP options parsed from the options section, or null when the section is empty.
    /// A snapshot parsed at construction.
    /// </summary>
    public List<DhcpOption>? Options { get; private set; }

    /// <summary>
    /// Gets the subnet mask parsed from DHCP option 1, or null when the option is absent.
    /// A snapshot parsed at construction.
    /// </summary>
    public Address? Subnet { get; private set; }

    /// <summary>
    /// Gets the first domain name server parsed from DHCP option 6, or null when the option is
    /// absent; any additional servers in the option are discarded. A snapshot parsed at construction.
    /// </summary>
    public Address? DNS { get; private set; }

    /// <summary>
    /// Gets the gateway address parsed from DHCP option 3 (Router), or null when the option is
    /// absent. A snapshot parsed at construction.
    /// </summary>
    public Address? Gateway { get; private set; }
}
