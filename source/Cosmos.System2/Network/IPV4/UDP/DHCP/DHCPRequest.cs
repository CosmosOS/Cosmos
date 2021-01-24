using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// DHCPRequest class.
    /// </summary>
    internal class DHCPRequest : DHCPPacket
    {

        /// <summary>
        /// Create new instance of the <see cref="DHCPRequest"/> class.
        /// </summary>
        internal DHCPRequest() : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="DHCPRequest"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPRequest(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Create new instance of the <see cref="DHCPRequest"/> class.
        /// </summary>
        /// <param name="mac_src">Source MAC Address.</param>
        /// <param name="RequestedAddress">Requested Address.</param>
        /// <param name="DHCPServerAddress">DHCP server IPv4 Address.</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal DHCPRequest(MACAddress mac_src, Address RequestedAddress, Address DHCPServerAddress) : base(mac_src, 22)
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

            RawData[291] = 54;
            RawData[292] = 4;

            RawData[293] = DHCPServerAddress.address[0];
            RawData[294] = DHCPServerAddress.address[1];
            RawData[295] = DHCPServerAddress.address[2];
            RawData[296] = DHCPServerAddress.address[3];

            //Parameters start here
            RawData[297] = 0x37;
            RawData[298] = 4;

            //Parameters
            RawData[299] = 0x01;
            RawData[300] = 0x03;
            RawData[301] = 0x0f;
            RawData[302] = 0x06;

            RawData[303] = 0xff; //ENDMARK
        }
    }
}
