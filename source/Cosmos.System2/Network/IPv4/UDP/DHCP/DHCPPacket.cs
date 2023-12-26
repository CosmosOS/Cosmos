/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP Packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*/

using Cosmos.HAL;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// Represents a DHCP option.
    /// </summary>
    public class DHCPOption
    {
        /// <summary>
        /// The type of the <see cref="DHCPOption"/>.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// The length of the <see cref="DHCPOption"/>.
        /// </summary>
        public byte Length { get; set; }

        /// <summary>
        /// The raw data of the <see cref="DHCPOption"/>.
        /// </summary>
        public byte[] Data { get; set; }
    }

    /// <summary>
    /// Represents a DHCP packet.
    /// </summary>
    public class DHCPPacket : UDPPacket
    {
        readonly int id;

        /// <summary>
        /// Handles a single DHCP packet.
        /// </summary>
        /// <param name="packetData">The packet data.</param>
        /// <exception cref="OverflowException">Thrown if UDP_Data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        public static void DHCPHandler(byte[] packetData)
        {
            var dhcpPacket = new DHCPPacket(packetData);

            var receiver = UdpClient.GetClient(dhcpPacket.DestinationPort);
            receiver?.ReceiveData(dhcpPacket);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPPacket"/> class.
        /// </summary>
        internal DHCPPacket() : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPPacket"/> class.
        /// </summary>
        /// <param name="mac_src">Source MAC Address.</param>
        /// <param name="dhcpDataSize">DHCP Data size</param>
        internal DHCPPacket(MACAddress mac_src, ushort dhcpDataSize)
            : this(Address.Zero, Address.Broadcast, mac_src, dhcpDataSize)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPPacket"/> class.
        /// </summary>
        /// <param name="client">Client IPv4 Address.</param>
        /// <param name="server">Server IPv4 Address.</param>
        /// <param name="sourceMAC">Source MAC Address.</param>
        /// <param name="dhcpDataSize">DHCP Data size</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal DHCPPacket(Address client, Address server, MACAddress sourceMAC, ushort dhcpDataSize)
            : base(client, server, 68, 67, (ushort)(dhcpDataSize + 240), MACAddress.Broadcast)
        {
            RawData[42] = 0x01; // Request
            RawData[43] = 0x01; // ethernet
            RawData[44] = 0x06; // Length mac
            RawData[45] = 0x00; // hops

            var rnd = new Random();
            id = rnd.Next(0, Int32.MaxValue);
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
            RawData[70] = sourceMAC.bytes[0];
            RawData[71] = sourceMAC.bytes[1];
            RawData[72] = sourceMAC.bytes[2];
            RawData[73] = sourceMAC.bytes[3];
            RawData[74] = sourceMAC.bytes[4];
            RawData[75] = sourceMAC.bytes[5];

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
        /// Init DHCPPacket fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void InitializeFields()
        {
            base.InitializeFields();
            MessageType = RawData[42];

            if (RawData[58] != 0)
            {
                Client = new Address(RawData, 58);
            }

            if (RawData[282] != 0)
            {
                Options = new List<DHCPOption>();

                for (int i = 0; i < RawData.Length - 282 && RawData[282 + i] != 0xFF; i += 2) //0xFF is DHCP packet end
                {
                    var option = new DHCPOption();
                    option.Type = RawData[282 + i];
                    option.Length = RawData[282 + i + 1];
                    option.Data = new byte[option.Length];
                    for (int j = 0; j < option.Length; j++)
                    {
                        option.Data[j] = RawData[282 + i + j + 2];
                    }
                    Options.Add(option);

                    i += option.Length;
                }

                foreach (var option in Options)
                {
                    if (option.Type == 1) //Mask
                    {
                        Subnet = new Address(option.Data, 0);
                    }
                    else if (option.Type == 3) //Router
                    {
                        Server = new Address(option.Data, 0);
                    }
                    else if (option.Type == 6) //DNS
                    {
                        DNS = new Address(option.Data, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the DHCP message type.
        /// </summary>
        internal byte MessageType { get; private set; }

        /// <summary>
        /// Gets the client IPv4 address.
        /// </summary>
        internal Address Client { get; private set; }

        /// <summary>
        /// Gets the DHCP options.
        /// </summary>
        internal List<DHCPOption> Options { get; private set; }

        /// <summary>
        /// Get Subnet IPv4 Address
        /// </summary>
        internal Address Subnet { get; private set; }

        /// <summary>
        /// Get DNS IPv4 Address
        /// </summary>
        internal Address DNS { get; private set; }

        /// <summary>
        /// Get DHCP Server IPv4 Address
        /// </summary>
        internal Address Server { get; private set; }
    }
}