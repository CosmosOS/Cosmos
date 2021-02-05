using System;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// DHCPDiscover class.
    /// </summary>
    internal class DHCPDiscover : DHCPPacket
    {
        /// <summary>
        /// Create new instance of the <see cref="DHCPDiscover"/> class.
        /// </summary>
        internal DHCPDiscover() : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="DHCPDiscover"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPDiscover(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Create new instance of the <see cref="DHCPDiscover"/> class.
        /// </summary>
        /// <param name="mac_src">Source MAC Address.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal DHCPDiscover(MACAddress mac_src) : base(mac_src, 10) //discover packet size
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
}
