using System;
using Cosmos.HAL.Network;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Represents an IP (Internet Protocol) packet.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="EthernetPacket"/>.
    /// </remarks>
    public class IPPacket : EthernetPacket
    {
        protected byte ipHeaderLength;
        private static ushort sNextFragmentID;

        /// <summary>
        /// Handles a single IPv4 packet.
        /// </summary>
        /// <param name="packetData">The raw data of the packet.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="OverflowException">Thrown if packetData array length is greater than Int32.MaxValue.</exception>
        internal static void IPv4Handler(byte[] packetData)
        {
            var ipPacket = new IPPacket(packetData);

            if (ipPacket.SourceIP == null)
            {
                NetworkStack.Debugger.Send("SourceIP null in IPv4Handler!");
            }

            ARPCache.Update(ipPacket.SourceIP, ipPacket.SourceMAC);

            if (NetworkStack.AddressMap.ContainsKey(ipPacket.DestinationIP.Hash) == true ||
                ipPacket.DestinationIP.Parts[3] == 255 // this is wrong x.x.x.255 is not always broadcast
                )
            {
                switch (ipPacket.Protocol)
                {
                    case 1:
                        ICMPPacket.ICMPHandler(packetData);
                        break;
                    case 6:
                        TCPPacket.TCPHandler(packetData);
                        break;
                    case 17:
                        UDPPacket.UDPHandler(packetData);
                        break;
                }
            }
            else if (NetworkStack.MACMap.ContainsKey(ipPacket.DestinationMAC.Hash))
            {
                DHCPPacket.DHCPHandler(packetData);
            }
        }

        /// <summary>
        /// Gets the next IP fragment ID.
        /// </summary>
        public static ushort NextIPFragmentID => sNextFragmentID++;

        /// <summary>
        /// Create new instance of the <see cref="IPPacket"/> class.
        /// </summary>
        internal IPPacket()
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="IPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public IPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Initializes all internal fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void InitializeFields()
        {
            base.InitializeFields();
            IPVersion = (byte)((RawData[14] & 0xF0) >> 4);
            ipHeaderLength = (byte)(RawData[14] & 0x0F);
            TypeOfService = RawData[15];
            IPLength = (ushort)((RawData[16] << 8) | RawData[17]);
            FragmentID = (ushort)((RawData[18] << 8) | RawData[19]);
            IPFlags = (byte)((RawData[20] & 0xE0) >> 5);
            FragmentOffset = (ushort)(((RawData[20] & 0x1F) << 8) | RawData[21]);
            TTL = RawData[22];
            Protocol = RawData[23];
            IPCRC = (ushort)((RawData[24] << 8) | RawData[25]);
            SourceIP = new Address(RawData, 26);
            DestinationIP = new Address(RawData, 30);
            DataOffset = (ushort)(14 + HeaderLength);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPPacket"/> class.
        /// </summary>
        /// <param name="dataLength">Data length.</param>
        /// <param name="protocol">Protocol.</param>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destionation address.</param>
        /// <param name="Flags">Flags.</param>
        protected IPPacket(ushort dataLength, byte protocol, Address source, Address dest, byte Flags)
            : this(MACAddress.None, MACAddress.None, dataLength, protocol, source, dest, Flags)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPPacket"/> class.
        /// </summary>
        /// <param name="dataLength">Data length.</param>
        /// <param name="protocol">Protocol.</param>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destionation address.</param>
        /// <param name="Flags">Flags.</param>
        /// /// <param name="broadcast">Mac address</param>
        protected IPPacket(ushort dataLength, byte protocol, Address source, Address dest, byte Flags, MACAddress broadcast)
            : this(MACAddress.None, broadcast, dataLength, protocol, source, dest, Flags)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPPacket"/> class.
        /// </summary>
        /// <param name="srcMAC">Source MAC address.</param>
        /// <param name="destMAC">Destination MAC address.</param>
        /// <param name="dataLength">Data length.</param>
        /// <param name="protocol">Protocol.</param>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destionation address.</param>
        /// <param name="Flags">Flags.</param>
        /// <exception cref="sys.ArgumentException">Thrown if RawData is invalid or null.</exception>
        public IPPacket(MACAddress srcMAC, MACAddress destMAC, ushort dataLength, byte protocol,
            Address source, Address dest, byte Flags)
            : base(destMAC, srcMAC, 0x0800, dataLength + 14 + 20)
        {
            RawData[14] = 0x45;
            RawData[15] = 0;
            IPLength = (ushort)(dataLength + 20);
            ipHeaderLength = 5;

            RawData[16] = (byte)((IPLength >> 8) & 0xFF);
            RawData[17] = (byte)((IPLength >> 0) & 0xFF);
            FragmentID = NextIPFragmentID;
            RawData[18] = (byte)((FragmentID >> 8) & 0xFF);
            RawData[19] = (byte)((FragmentID >> 0) & 0xFF);
            RawData[20] = Flags;
            RawData[21] = 0x00;
            RawData[22] = 0x80;
            RawData[23] = protocol;
            RawData[24] = 0x00;
            RawData[25] = 0x00;
            for (int b = 0; b < 4; b++)
            {
                RawData[26 + b] = source.Parts[b];
                RawData[30 + b] = dest.Parts[b];
            }
            IPCRC = CalcIPCRC(20);
            RawData[24] = (byte)((IPCRC >> 8) & 0xFF);
            RawData[25] = (byte)((IPCRC >> 0) & 0xFF);

            InitializeFields();
        }

        /// <summary>
        /// Calculates the CRC of the packet.
        /// </summary>
        /// <param name="offset">The offset, in bytes.</param>
        /// <param name="length">The length, in bytes.</param>
        protected ushort CalcOcCRC(ushort offset, ushort length) => CalcOcCRC(RawData, offset, length);

        /// <summary>
        /// Calculates the CRC of the packet.
        /// </summary>
        /// <param name="buffer">The buffer to use.</param>
        /// <param name="offset">The offset, in bytes.</param>
        /// <param name="length">The length, in bytes.</param>
        protected static ushort CalcOcCRC(byte[] buffer, ushort offset, int length)
        {
            return (ushort)~SumShortValues(buffer, offset, length);
        }

        protected static ushort SumShortValues(byte[] buffer, int offset, int length)
        {
            uint chksum = 0;
            int end = offset + (length & ~1);
            int i = offset;

            while (i != end)
            {
                chksum += (uint)(((ushort)buffer[i++] << 8) + (ushort)buffer[i++]);
            }
            if (i != offset + length)
            {
                chksum += (uint)((ushort)buffer[i] << 8);
            }
            chksum = (chksum & 0xFFFF) + (chksum >> 16);
            chksum = (chksum & 0xFFFF) + (chksum >> 16);
            return (ushort)chksum;
        }

        /// <summary>
        /// Calculates the CRC of the packet.
        /// </summary>
        /// <param name="headerLength">The length of the header, in bytes.</param>
        protected ushort CalcIPCRC(ushort headerLength)
        {
            return CalcOcCRC(14, headerLength);
        }

        /// <summary>
        /// Gets the IP version of the packet.
        /// </summary>
        internal byte IPVersion { get; private set; }

        /// <summary>
        /// Gets the length of the header, in bytes.
        /// </summary>
        internal ushort HeaderLength => (ushort)(ipHeaderLength * 4);

        /// <summary>
        /// Gets the type of service.
        /// </summary>
        internal byte TypeOfService { get; private set; }

        /// <summary>
        /// Gets the IP length of the packet.
        /// </summary>
        internal ushort IPLength { get; private set; }

        /// <summary>
        /// Gets the fragment ID.
        /// </summary>
        internal ushort FragmentID { get; private set; }

        /// <summary>
        /// Gets the flags of the packet.
        /// </summary>
        internal byte IPFlags { get; private set; }

        /// <summary>
        /// Gets the fragment offset.
        /// </summary>
        internal ushort FragmentOffset { get; private set; }

        /// <summary>
        /// Gets the TTL (Time-To-Live) of the packet.
        /// </summary>
        internal byte TTL { get; private set; }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        internal byte Protocol { get; private set; }

        /// <summary>
        /// Gets the IPCRC.
        /// </summary>
        internal ushort IPCRC { get; private set; }

        /// <summary>
        /// Gets the source IP address.
        /// </summary>
        internal Address SourceIP { get; private set; }

        /// <summary>
        /// Gets the destination IP address.
        /// </summary>
        internal Address DestinationIP { get; private set; }

        /// <summary>
        /// Gets the offset of the data.
        /// </summary>
        internal ushort DataOffset { get; private set; }

        /// <summary>
        /// Gets the length of the data.
        /// </summary>
        internal ushort DataLength => (ushort)(IPLength - HeaderLength);

        public override string ToString()
        {
            return "IP Packet Src=" + SourceIP + ", Dest=" + DestinationIP + ", Protocol=" + Protocol + ", TTL=" + TTL + ", DataLen=" + DataLength;
        }
    }
}
