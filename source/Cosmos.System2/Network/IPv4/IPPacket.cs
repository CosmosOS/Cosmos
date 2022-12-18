/*
* PROJECT:          Aura Operating System Development
* CONTENT:          IPv4 Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Cosmos.HAL;
using Cosmos.HAL.Network;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// IPPacket class. See also: <seealso cref="EthernetPacket"/>.
    /// </summary>
    public class IPPacket : EthernetPacket
    {
        protected byte ipHeaderLength;

        private static ushort sNextFragmentID;

        /// <summary>
        /// IPv4 handler.
        /// </summary>
        /// <param name="packetData">Packet data.</param>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="sys.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="sys.ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="sys.OverflowException">Thrown if packetData array length is greater than Int32.MaxValue.</exception>
        internal static void IPv4Handler(byte[] packetData)
        {
            var ip_packet = new IPPacket(packetData);

            if (ip_packet.SourceIP == null)
            {
                Global.mDebugger.Send("SourceIP null in IPv4Handler!");
            }
            ARPCache.Update(ip_packet.SourceIP, ip_packet.SourceMAC);

            if (NetworkStack.AddressMap.ContainsKey(ip_packet.DestinationIP.Hash) == true ||
                ip_packet.DestinationIP.address[3] == 255)
            {
                switch (ip_packet.Protocol)
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
            else if (NetworkStack.MACMap.ContainsKey(ip_packet.DestinationMAC.Hash))
            {
                DHCPPacket.DHCPHandler(packetData);
            }
        }

        /// <summary>
        /// Get next IP fragment ID.
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
        /// Init IPPacket fields.
        /// </summary>
        /// <exception cref="sys.ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void InitFields()
        {
            base.InitFields();
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
        /// Create new instance of the <see cref="IPPacket"/> class.
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
        /// Create new instance of the <see cref="IPPacket"/> class.
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
        /// Create new instance of the <see cref="IPPacket"/> class.
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
                RawData[26 + b] = source.address[b];
                RawData[30 + b] = dest.address[b];
            }
            IPCRC = CalcIPCRC(20);
            RawData[24] = (byte)((IPCRC >> 8) & 0xFF);
            RawData[25] = (byte)((IPCRC >> 0) & 0xFF);

            InitFields();
        }

        /// <summary>
        /// Calcutale CRC.
        /// </summary>
        /// <param name="offset">Offset.</param>
        /// <param name="length">Length.</param>
        /// <returns></returns>
        protected ushort CalcOcCRC(ushort offset, ushort length) => CalcOcCRC(RawData, offset, length);

        /// <summary>
        /// Calcutale CRC.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="length">Length.</param>
        /// <returns>ushort value.</returns>
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
        /// Calcutale CRC.
        /// </summary>
        /// <param name="headerLength">Header length.</param>
        /// <returns>ushort value.</returns>
        protected ushort CalcIPCRC(ushort headerLength)
        {
            return CalcOcCRC(14, headerLength);
        }

        /// <summary>
        /// Get IP version.
        /// </summary>
        internal byte IPVersion { get; private set; }
        /// <summary>
        /// Get header length.
        /// </summary>
        internal ushort HeaderLength => (ushort)(ipHeaderLength * 4);

        /// <summary>
        /// Get type of service.
        /// </summary>
        internal byte TypeOfService { get; private set; }

        /// <summary>
        /// Get IP length.
        /// </summary>
        internal ushort IPLength { get; private set; }
        /// <summary>
        /// Get fragment ID.
        /// </summary>
        internal ushort FragmentID { get; private set; }
        /// <summary>
        /// Get flags.
        /// </summary>
        internal byte IPFlags { get; private set; }
        /// <summary>
        /// Get fragment offset.
        /// </summary>
        internal ushort FragmentOffset { get; private set; }
        /// <summary>
        /// Get TTL.
        /// </summary>
        internal byte TTL { get; private set; }
        /// <summary>
        /// Get protocol.
        /// </summary>
        internal byte Protocol { get; private set; }
        /// <summary>
        /// Get IPCRC.
        /// </summary>
        internal ushort IPCRC { get; private set; }

        /// <summary>
        /// Get source IP.
        /// </summary>
        internal Address SourceIP { get; private set; }

        /// <summary>
        /// Get destination IP.
        /// </summary>
        internal Address DestinationIP { get; private set; }

        /// <summary>
        /// Get data offset.
        /// </summary>
        internal ushort DataOffset { get; private set; }

        /// <summary>
        /// Get data length.
        /// </summary>
        internal ushort DataLength => (ushort)(IPLength - HeaderLength);

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "IP Packet Src=" + SourceIP + ", Dest=" + DestinationIP + ", Protocol=" + Protocol + ", TTL=" + TTL + ", DataLen=" + DataLength;
        }
    }
}
