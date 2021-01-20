using Cosmos.HAL;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;
using System;
using System.Collections.Generic;
using System.Text;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP Packet
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{

    /// <summary>
    /// DHCP Option
    /// </summary>
    public class DHCPOption
    {
        /// <summary>
        /// DHCP Option Type
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// DHCP Option Length
        /// </summary>
        public byte Length { get; set; }

        /// <summary>
        /// DHCP Option Data
        /// </summary>
        public byte[] Data { get; set; }
    }

    /// <summary>
    /// DHCPPacket class.
    /// </summary>
    public class DHCPPacket : UDPPacket
    {
        int xID;

        /// <summary>
        /// DHCP handler.
        /// </summary>
        /// <param name="packetData">Packet data.</param>
        /// <exception cref="OverflowException">Thrown if UDP_Data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="sysIO.IOException">Thrown on IO error.</exception>
        public static void DHCPHandler(byte[] packetData)
        {
            DHCPPacket dhcp_packet = new DHCPPacket(packetData);

            DHCPClient receiver = DHCPClient.currentClient;
            if (receiver != null)
            {
                receiver.receiveData(dhcp_packet);
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new DHCPPacket();
        }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPPacket"/> class.
        /// </summary>
        internal DHCPPacket() : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public DHCPPacket(byte[] rawData)
            : base(rawData)
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPPacket"/> class.
        /// </summary>
        /// <param name="mac_src">Source MAC Address.</param>
        /// <param name="dhcpDataSize">DHCP Data size</param>
        internal DHCPPacket(MACAddress mac_src, ushort dhcpDataSize)
            : this(Address.Zero, Address.Broadcast, mac_src, dhcpDataSize)
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPPacket"/> class.
        /// </summary>
        /// <param name="client">Client IPv4 Address.</param>
        /// <param name="server">Server IPv4 Address.</param>
        /// <param name="mac_src">Source MAC Address.</param>
        /// <param name="dhcpDataSize">DHCP Data size</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal DHCPPacket(Address client, Address server, MACAddress mac_src, ushort dhcpDataSize)
            : base(client, server, 68, 67, (ushort)(dhcpDataSize + 240), MACAddress.Broadcast)
        {
            //Request
            RawData[42] = 0x01;

            //ethernet
            RawData[43] = 0x01;

            //Length mac
            RawData[44] = 0x06;

            //hops
            RawData[45] = 0x00;

            Random rnd = new Random();
            xID = rnd.Next(0, Int32.MaxValue);
            RawData[46] = (byte)((xID >> 24) & 0xFF);
            RawData[47] = (byte)((xID >> 16) & 0xFF);
            RawData[48] = (byte)((xID >> 8) & 0xFF);
            RawData[49] = (byte)((xID >> 0) & 0xFF);

            //second elapsed
            RawData[50] = 0x00;
            RawData[51] = 0x00;

            //option bootp
            RawData[52] = 0x00;
            RawData[53] = 0x00;

            //client ip address
            RawData[54] = client.address[0];
            RawData[55] = client.address[1];
            RawData[56] = client.address[2];
            RawData[57] = client.address[3];

            for (int i = 0; i < 13; i++)
            {
                RawData[58 + i] = 0x00;
            }

            //Src mac
            RawData[70] = mac_src.bytes[0];
            RawData[71] = mac_src.bytes[1];
            RawData[72] = mac_src.bytes[2];
            RawData[73] = mac_src.bytes[3];
            RawData[74] = mac_src.bytes[4];
            RawData[75] = mac_src.bytes[5];

            //Fill 0
            for (int i = 0; i < 202; i++)
            {
                RawData[76 + i] = 0x00;
            }

            //DHCP Magic cookie
            RawData[278] = 0x63;
            RawData[279] = 0x82;
            RawData[280] = 0x53;
            RawData[281] = 0x63;

            initFields();
        }

        /// <summary>
        /// Init DHCPPacket fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void initFields()
        {
            base.initFields();
            MessageType = RawData[42];
            Client = new Address(RawData, 58);
            Server = new Address(RawData, 62);

            if (RawData[282] != 0)
            {
                Options = new List<DHCPOption>();

                for (int i = 0; i < RawData.Length - 282 && RawData[282 + i] != 0xFF; i += 2) //0xFF is DHCP packet end
                {
                    DHCPOption option = new DHCPOption();
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
            }
        }

        /// <summary>
        /// Get DHCP message type
        /// </summary>
        internal byte MessageType { get; private set; }

        /// <summary>
        /// Get Client IPv4 Address
        /// </summary>
        internal Address Client { get; private set; }

        /// <summary>
        /// Get DHCP Server IPv4 Address
        /// </summary>
        internal Address Server { get; private set; }

        /// <summary>
        /// Get DHCP Options
        /// </summary>
        internal List<DHCPOption> Options { get; private set; }

    }

    /// <summary>
    /// DHCPDiscover class.
    /// </summary>
    internal class DHCPDiscover : DHCPPacket
    {
        /// <summary>
        /// Create new inctanse of the <see cref="DHCPDiscover"/> class.
        /// </summary>
        internal DHCPDiscover() : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPDiscover"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPDiscover(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPDiscover"/> class.
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

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPDiscover();
        }

        protected override void initFields()
        {
            base.initFields();
        }

    }

    /// <summary>
    /// DHCPRequest class.
    /// </summary>
    internal class DHCPRequest : DHCPPacket
    {

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPRequest"/> class.
        /// </summary>
        internal DHCPRequest() : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPRequest"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPRequest(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPRequest"/> class.
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

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPRequest();
        }

        /// <summary>
        /// Init DHCPRequest fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void initFields()
        {
            base.initFields();
        }

    }

    /// <summary>
    /// DHCPAck class.
    /// </summary>
    internal class DHCPAck : DHCPPacket
    {
        /// <summary>
        /// Create new inctanse of the <see cref="DHCPAck"/> class.
        /// </summary>
        internal DHCPAck() : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPAck"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPAck(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPAck();
        }

        /// <summary>
        /// Init DHCPAck fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void initFields()
        {
            base.initFields();

            foreach (var option in Options)
            {
                if (option.Type == 1) //Mask
                {
                    Subnet = new Address(option.Data, 0);
                }
                else if (option.Type == 6) //DNS
                {
                    DNS = new Address(option.Data, 0);
                }
            }
        }

        /// <summary>
        /// Get Subnet IPv4 Address
        /// </summary>
        internal Address Subnet { get; private set; }

        /// <summary>
        /// Get DNS IPv4 Address
        /// </summary>
        internal Address DNS { get; private set; }

    }

    /// <summary>
    /// DHCPRelease class.
    /// </summary>
    internal class DHCPRelease : DHCPPacket
    {
        /// <summary>
        /// Create new inctanse of the <see cref="DHCPRelease"/> class.
        /// </summary>
        internal DHCPRelease() : base()
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPRelease"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPRelease(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="DHCPRelease"/> class.
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

            RawData[287] = server.address[0];
            RawData[288] = server.address[1];
            RawData[289] = server.address[2];
            RawData[290] = server.address[3];

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

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new DHCPRelease();
        }

        /// <summary>
        /// Init DHCPRelease fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void initFields()
        {
            base.initFields();
        }

    }
}
