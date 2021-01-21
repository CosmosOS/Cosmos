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

            UdpClient receiver = UdpClient.GetClient(udp_packet.DestinationPort);
            if (receiver != null)
            {
                receiver.ReceiveData(udp_packet);
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="UDPPacket"/> class.
        /// </summary>
        internal UDPPacket()
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="UDPPacket"/> class.
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
            InitFields();
        }

        public UDPPacket(Address source, Address dest, UInt16 srcport, UInt16 destport, UInt16 datalength, MACAddress destmac)
            : base((ushort)(datalength + 8), 17, source, dest, 0x00, destmac)
        {
            MakePacket(srcport, destport, datalength);
            InitFields();
        }

        /// <summary>
        /// Create new instance of the <see cref="UDPPacket"/> class.
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

            InitFields();
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
        protected override void InitFields()
        {
            base.InitFields();
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
