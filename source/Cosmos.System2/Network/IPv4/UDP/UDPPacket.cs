using Cosmos.HAL.Network;
using System;

namespace Cosmos.System.Network.IPv4.UDP
{
    /// <summary>
    /// Represents a UDP packet.
    /// </summary>
    public class UDPPacket : IPPacket
    {
        private ushort udpCRC;

        /// <summary>
        /// Handles UDP packets.
        /// </summary>
        /// <param name="packetData">The raw packet data.</param>
        /// <exception cref="OverflowException">Thrown if UDP_Data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>

        internal static void UDPHandler(byte[] packetData)
        {
            var udpPacket = new UDPPacket(packetData);

            NetworkStack.Debugger.Send("[Received] UDP packet from " + udpPacket.SourceIP.ToString() + ":" + udpPacket.SourcePort.ToString());

            if (udpPacket.SourcePort == 67)
            {
                DHCP.DHCPPacket.DHCPHandler(packetData);
                return;
            }
            else if (udpPacket.SourcePort == 53)
            {
                DNS.DNSPacket.DNSHandler(packetData);
                return;
            }

            var receiver = UdpClient.GetClient(udpPacket.DestinationPort);
            receiver?.ReceiveData(udpPacket);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UDPPacket"/> class.
        /// </summary>
        internal UDPPacket()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UDPPacket"/> class.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        public UDPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        public UDPPacket(Address source, Address dest, ushort srcport, ushort destport, ushort datalength)
            : base((ushort)(datalength + 8), 17, source, dest, 0x00)
        {
            MakePacket(srcport, destport, datalength);
            InitializeFields();
        }

        public UDPPacket(Address source, Address dest, ushort srcport, ushort destport, ushort datalength, MACAddress destmac)
            : base((ushort)(datalength + 8), 17, source, dest, 0x00, destmac)
        {
            MakePacket(srcport, destport, datalength);
            InitializeFields();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UDPPacket"/> class.
        /// </summary>
        /// <param name="source">The source address.</param>
        /// <param name="dest">The destination address.</param>
        /// <param name="srcPort">The source port.</param>
        /// <param name="destPort">The destination port.</param>
        /// <param name="data">The data array.</param>
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

            InitializeFields();
        }

        private void MakePacket(ushort srcport, ushort destport, ushort length)
        {
            RawData[this.DataOffset + 0] = (byte)((srcport >> 8) & 0xFF);
            RawData[this.DataOffset + 1] = (byte)((srcport >> 0) & 0xFF);
            RawData[this.DataOffset + 2] = (byte)((destport >> 8) & 0xFF);
            RawData[this.DataOffset + 3] = (byte)((destport >> 0) & 0xFF);
            UDPLength = (ushort)(length + 8);

            RawData[this.DataOffset + 4] = (byte)((UDPLength >> 8) & 0xFF);
            RawData[this.DataOffset + 5] = (byte)((UDPLength >> 0) & 0xFF);

            RawData[this.DataOffset + 6] = (byte)((0 >> 8) & 0xFF);
            RawData[this.DataOffset + 7] = (byte)((0 >> 0) & 0xFF);
        }

        protected override void InitializeFields()
        {
            base.InitializeFields();
            SourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
            DestinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
            UDPLength = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
            udpCRC = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        }

        /// <summary>
        /// Gets the destination port.
        /// </summary>
        public ushort DestinationPort { get; private set; }

        /// <summary>
        /// Gets the source port.
        /// </summary>
        public ushort SourcePort { get; private set; }

        /// <summary>
        /// Gets the UDP length of the packet.
        /// </summary>
        public ushort UDPLength { get; private set; }

        /// <summary>
        /// Get UDP data length of the packet.
        /// </summary>
        public ushort UDPDataLength => (ushort)(UDPLength - 8);

        /// <summary>
        /// Gets the UDP data of the packet.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        internal byte[] UDPData
        {
            get
            {
                byte[] data = new byte[UDPDataLength];

                for (int b = 0; b < data.Length; b++)
                {
                    data[b] = RawData[DataOffset + 8 + b];
                }

                return data;
            }
        }

        public override string ToString()
        {
            return "UDP Packet Src=" + SourceIP + ":" + SourcePort + "," +
                   "Dest=" + DestinationIP + ":" + DestinationPort + ", DataLen=" + UDPDataLength;
        }
    }
}
