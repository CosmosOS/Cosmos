using System;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network.IPv4.UDP.DHCP;

/// <summary>
///     DHCPRequest class.
/// </summary>
internal class DHCPRequest : DHCPPacket
{
    /// <summary>
    ///     Create new instance of the <see cref="DHCPRequest" /> class.
    /// </summary>
    internal DHCPRequest()
    {
    }

    /// <summary>
    ///     Create new instance of the <see cref="DHCPRequest" /> class.
    /// </summary>
    /// <param name="rawData">Raw data.</param>
    internal DHCPRequest(byte[] rawData) : base(rawData)
    {
    }

    /// <summary>
    ///     Create new instance of the <see cref="DHCPRequest" /> class.
    /// </summary>
    /// <param name="mac_src">Source MAC Address.</param>
    /// <param name="RequestedAddress">Requested Address.</param>
    /// <param name="DHCPServerAddress">DHCP server IPv4 Address.</param>
    /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
    /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
    internal DHCPRequest(MACAddress mac_src, Address RequestedAddress) : base(mac_src, 16)
    {
        //Request
        RawData[282] = 53;
        RawData[283] = 1;
        RawData[284] = 3;

        //Requested Address
        RawData[285] = 50;
        RawData[286] = 4;

        RawData[287] = RequestedAddress.address[0];
        RawData[288] = RequestedAddress.address[1];
        RawData[289] = RequestedAddress.address[2];
        RawData[290] = RequestedAddress.address[3];

        //Parameters start here
        RawData[291] = 0x37;
        RawData[292] = 4;

        //Parameters
        RawData[293] = 0x01;
        RawData[294] = 0x03;
        RawData[295] = 0x0f;
        RawData[296] = 0x06;

        RawData[297] = 0xff; //ENDMARK
    }
}
