/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DHCP Release
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.UDP;

namespace Cosmos.Kernel.System.Network.IPv4.DHCP;

/// <summary>
/// Represents a DHCPRELEASE packet, sent to give an assigned address back to the server.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DhcpRelease : DhcpPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpRelease"/> class from received data.
    /// The packet aliases <paramref name="rawData"/> without copying.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame bytes, aliased rather than copied.</param>
    public DhcpRelease(byte[] rawData) : base(rawData)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpRelease"/> class, addressed to the DHCP
    /// server's unicast IP address rather than broadcast. Writes DHCP option 53 (message type)
    /// with value 7, DHCPRELEASE, then option 54 (server identifier) carrying
    /// <paramref name="server"/>, then option 61 (client identifier) carrying the Ethernet
    /// hardware type and <paramref name="source"/>, then the end mark.
    /// </summary>
    /// <param name="client">The client's currently assigned IPv4 address, used as the source.</param>
    /// <param name="server">The DHCP server's IPv4 address, used as the destination and written to option 54.</param>
    /// <param name="source">The MAC address of the sending network device, written to option 61.</param>
    public DhcpRelease(Address client, Address server, MACAddress source) : base(client, server, source, 19)
    {
        //Release
        RawData[282] = 0x35;
        RawData[283] = 0x01;
        RawData[284] = 0x07;

        //DHCP Server ID
        RawData[285] = 0x36;
        RawData[286] = 0x04;

        RawData[287] = server.Parts[0];
        RawData[288] = server.Parts[1];
        RawData[289] = server.Parts[2];
        RawData[290] = server.Parts[3];

        //Client ID
        RawData[291] = 0x3d;
        RawData[292] = 7;
        RawData[293] = 1;

        RawData[294] = source._bytes[0];
        RawData[295] = source._bytes[1];
        RawData[296] = source._bytes[2];
        RawData[297] = source._bytes[3];
        RawData[298] = source._bytes[4];
        RawData[299] = source._bytes[5];

        RawData[300] = 0xff; //ENDMARK
    }
}
