/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UDP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.HAL;
using Cosmos.HAL.Network;
using System;
using System.Text;

namespace Cosmos.System.Network.IPv4.UDP
{
    /// <summary>
    /// UDPPacket class.
    /// </summary>
    public class UDPPacket : IPPacket
    {
        /// <summary>
        /// UDP CRC.
        /// </summary>
        private ushort udpCRC;

        /// <summary>
        /// UDP handler.
        /// </summary>
        /// <param name="packetData">Packet data.</param>
        /// <exception cref="OverflowException">Thrown if UDP_Data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="sysIO.IOException">Thrown on IO error.</exception>

        internal static void UDPHandler(byte[] packetData)
        {
            UDPPacket udp_packet = new UDPPacket(packetData);

            Global.mDebugger.Send("[Received] UDP packet from " + udp_packet.SourceIP.ToString() + ":" + udp_packet.SourcePort.ToString());

            if (udp_packet.SourcePort == 67)
            {
                DHCP.DHCPPacket.DHCPHandler(packetData);
                return;
            }
            else if (udp_packet.SourcePort == 53)
            {
                DNS.DNSPacket.DNSHandler(packetData);
                return;
            }

            UdpClient receiver = UdpClient.Client(udp_packet.DestinationPort);
            if (receiver != null)
            {
                receiver.receiveData(udp_packet);
            }
        }

        /// <summary>
        /// Make header.
        /// </summary>
        /// <param name="sourceIP">Source IP.</param>
        /// <param name="destIP">Destination IP.</param>
        /// <param name="udpLen">UDP length.</param>
        /// <param name="sourcePort">Source port.</param>
        /// <param name="destPort">Destination port.</param>
        /// <param name="UDP_Data">UDP data.</param>
        /// <returns>byte array value.</returns>
        /// <exception cref="OverflowException">Thrown if UDP_Data array length is greater than Int32.MaxValue.</exception>
        public static byte[] MakeHeader(byte[] sourceIP, byte[] destIP, UInt16 udpLen, UInt16 sourcePort, UInt16 destPort, byte[] UDP_Data)
        {
            byte[] header = new byte[18 + UDP_Data.Length];

            header[0] = sourceIP[0];
            header[1] = sourceIP[1];
            header[2] = sourceIP[2];
            header[3] = sourceIP[3];

            header[4] = destIP[0];
            header[5] = destIP[1];
            header[6] = destIP[2];
            header[7] = destIP[3];

            header[8] = 0x00;

            header[9] = 0x11;

            header[10] = (byte)((udpLen >> 8) & 0xFF);
            header[11] = (byte)((udpLen >> 0) & 0xFF);

            header[12] = (byte)((sourcePort >> 8) & 0xFF);
            header[13] = (byte)((sourcePort >> 0) & 0xFF);

            header[14] = (byte)((destPort >> 8) & 0xFF);
            header[15] = (byte)((destPort >> 0) & 0xFF);

            header[16] = (byte)((udpLen >> 8) & 0xFF);
            header[17] = (byte)((udpLen >> 0) & 0xFF);

            for (int i = 0; i < UDP_Data.Length; i++)
            {
                header[18 + i] = UDP_Data[i];
            }

            return header;
        }

        /// <summary>
        /// Check CRC.
        /// </summary>
        /// <param name="packet">Packer to be checked.</param>
        /// <returns>bool value.</returns>
        /// <exception cref="OverflowException">Thrown if packet.UDP_Data lenght if greater than Int32.MaxValue.</exception>
        public static bool CheckCRC(UDPPacket packet)
        {
            byte[] header = MakeHeader(packet.SourceIP.address, packet.DestinationIP.address, packet.UDP_Length, packet.SourcePort, packet.DestinationPort, packet.UDP_Data);
            if (CalcOcCRC(header, 0, header.Length) == packet.udpCRC)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new UDPPacket();
        }

        /// <summary>
        /// Create new inctanse of the <see cref="UDPPacket"/> class.
        /// </summary>
        internal UDPPacket()
        {
        }

        /// <summary>
        /// Create new inctanse of the <see cref="UDPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public UDPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        public UDPPacket(Address source, Address dest, UInt16 srcport, UInt16 destport, UInt16 datalength)
            : base((ushort)(datalength + 8), 17, source, dest, 0x00)
        {
            MakePacket(srcport, destport, datalength);
            initFields();
        }

        public UDPPacket(Address source, Address dest, UInt16 srcport, UInt16 destport, UInt16 datalength, MACAddress destmac)
            : base((ushort)(datalength + 8), 17, source, dest, 0x00, destmac)
        {
            MakePacket(srcport, destport, datalength);
            initFields();
        }

        /// <summary>
        /// Create new inctanse of the <see cref="UDPPacket"/> class.
        /// </summary>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="srcPort">Source port.</param>
        /// <param name="destPort">Destination port.</param>
        /// <param name="data">Data array.</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        public UDPPacket(Address source, Address dest, ushort srcPort, ushort destPort, byte[] data)
            : base((ushort)(data.Length + 8), 17, source, dest, 0x00)
        {
            MakePacket(srcPort, destPort, (ushort)data.Length);

            for (int b = 0; b < data.Length; b++)
            {
                RawData[this.DataOffset + 8 + b] = data[b];
            }

            initFields();
        }

        private void MakePacket(ushort srcport, ushort destport, ushort length)
        {
            RawData[this.DataOffset + 0] = (byte)((srcport >> 8) & 0xFF);
            RawData[this.DataOffset + 1] = (byte)((srcport >> 0) & 0xFF);
            RawData[this.DataOffset + 2] = (byte)((destport >> 8) & 0xFF);
            RawData[this.DataOffset + 3] = (byte)((destport >> 0) & 0xFF);
            UDP_Length = (ushort)(length + 8);

            RawData[this.DataOffset + 4] = (byte)((UDP_Length >> 8) & 0xFF);
            RawData[this.DataOffset + 5] = (byte)((UDP_Length >> 0) & 0xFF);

            RawData[this.DataOffset + 6] = (byte)((0 >> 8) & 0xFF);
            RawData[this.DataOffset + 7] = (byte)((0 >> 0) & 0xFF);
        }

        /// <summary>
        /// Init UDPPacket fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void initFields()
        {
            base.initFields();
            SourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
            DestinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
            UDP_Length = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
            udpCRC = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        }

        /// <summary>
        /// Get destination port.
        /// </summary>
        public ushort DestinationPort { get; private set; }
        /// <summary>
        /// Get source port.
        /// </summary>
        public ushort SourcePort { get; private set; }
        /// <summary>
        /// Get UDP length.
        /// </summary>
        public ushort UDP_Length { get; private set; }
        /// <summary>
        /// Get UDP data lenght.
        /// </summary>
        public ushort UDP_DataLength => (ushort)(UDP_Length - 8);

        /// <summary>
        /// Get UDP data.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        internal byte[] UDP_Data
        {
            get
            {
                byte[] data = new byte[UDP_DataLength];

                for (int b = 0; b < data.Length; b++)
                {
                    data[b] = RawData[DataOffset + 8 + b];
                }

                return data;
            }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "UDP Packet Src=" + SourceIP + ":" + SourcePort + "," +
                   "Dest=" + DestinationIP + ":" + DestinationPort + ", DataLen=" + UDP_DataLength;
        }
    }
}
