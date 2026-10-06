// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DHCP Discover
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices;

namespace Cosmos.Kernel.System.Network.Protocols.IPv4.Dhcp;

/// <summary>
/// Represents a DHCPDISCOVER packet, broadcast to locate DHCP servers.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DhcpDiscover : DhcpPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpDiscover"/> class from received data.
    /// The packet aliases <paramref name="rawData"/> without copying.
    /// </summary>
    /// <param name="rawData">The raw Ethernet frame bytes, aliased rather than copied.</param>
    public DhcpDiscover(byte[] rawData) : base(rawData)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpDiscover"/> class as a broadcast discover.
    /// Writes DHCP option 53 (message type) with value 1, DHCPDISCOVER, then option 55, a parameter
    /// request list asking for options 1 (subnet mask), 3 (router), 15 (domain name) and
    /// 6 (domain name server), then the end mark.
    /// </summary>
    /// <param name="sourceMac">The MAC address of the sending network device.</param>
    public DhcpDiscover(MacAddress sourceMac) : base(sourceMac, 10) //discover packet size
    {
        //Discover
        RawData[282] = 0x35;
        RawData[283] = 0x01;
        RawData[284] = 0x01;

        //Parameters start here
        RawData[285] = 0x37;
        RawData[286] = 4;

        //Parameters*
        RawData[287] = 0x01;
        RawData[288] = 0x03;
        RawData[289] = 0x0f;
        RawData[290] = 0x06;

        RawData[291] = 0xff; //ENDMARK
    }
}
