// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DHCP Request
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices;

namespace Cosmos.Kernel.System.Network.Protocols.IPv4.Dhcp;

/// <summary>
/// Represents a DHCPREQUEST packet, broadcast to request a previously offered address.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DhcpRequest : DhcpPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpRequest"/> class from received data.
    /// The packet aliases <paramref name="rawData"/> without copying.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame bytes, aliased rather than copied.</param>
    public DhcpRequest(byte[] rawData) : base(rawData)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpRequest"/> class as a broadcast request.
    /// Writes DHCP option 53 (message type) with value 3, DHCPREQUEST, then option 50 carrying
    /// <paramref name="requestedAddress"/>, then option 55, a parameter request list asking for
    /// options 1 (subnet mask), 3 (router), 15 (domain name) and 6 (domain name server), then
    /// the end mark.
    /// </summary>
    /// <param name="sourceMac">The MAC address of the sending network device.</param>
    /// <param name="requestedAddress">The IPv4 address to request, written to option 50.</param>
    public DhcpRequest(MacAddress sourceMac, Address requestedAddress) : base(sourceMac, 16) // 3 (message type) + 6 (requested address) + 6 (parameter list) + 1 (end mark) option bytes
    {
        // Option 53, message type: DHCPREQUEST
        RawData[282] = 53;
        RawData[283] = 1;
        RawData[284] = 3;

        // Option 50, requested IP address
        RawData[285] = 50;
        RawData[286] = 4;

        RawData[287] = requestedAddress.Parts[0];
        RawData[288] = requestedAddress.Parts[1];
        RawData[289] = requestedAddress.Parts[2];
        RawData[290] = requestedAddress.Parts[3];

        // Option 55, parameter request list
        RawData[291] = 0x37;
        RawData[292] = 4;

        // Requested: subnet mask, router, domain name, domain name server
        RawData[293] = 0x01;
        RawData[294] = 0x03;
        RawData[295] = 0x0f;
        RawData[296] = 0x06;

        RawData[297] = 0xff; // End mark
    }
}
