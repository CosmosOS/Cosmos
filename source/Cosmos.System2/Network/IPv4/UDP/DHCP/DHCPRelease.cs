using System;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// Represents a DHCP release packet.
    /// </summary>
    internal class DHCPRelease : DHCPPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPRelease"/> class.
        /// </summary>
        internal DHCPRelease() : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPRelease"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPRelease(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPRelease"/> class.
        /// </summary>
        /// <param name="client">Client IPv4 Address.</param>
        /// <param name="server">DHCP Server IPv4 Address.</param>
        /// <param name="source">Source MAC Address.</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal DHCPRelease(Address client, Address server, MACAddress source) : base(client, server, source, 19)
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

            RawData[294] = source.bytes[0];
            RawData[295] = source.bytes[1];
            RawData[296] = source.bytes[2];
            RawData[297] = source.bytes[3];
            RawData[298] = source.bytes[4];
            RawData[299] = source.bytes[5];

            RawData[300] = 0xff; //ENDMARK
        }
    }
}
