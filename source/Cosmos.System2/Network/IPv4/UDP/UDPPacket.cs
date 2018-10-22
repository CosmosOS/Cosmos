using System;
using System.Text;

namespace Cosmos.System.Network.IPv4
{
    public class UDPPacket : IPPacket
    {
        private ushort udpCRC;

        internal static void UDPHandler(byte[] packetData)
        {
            UDPPacket udp_packet = new UDPPacket(packetData);

            NetworkStack.debugger.Send("Received UDP packet from " + udp_packet.SourceIP.ToString() + ":" + udp_packet.SourcePort.ToString());

            if (CheckCRC(udp_packet))
            {

                if (udp_packet.SourcePort == 68)
                {
                    //Network.DHCP.DHCPPacket.DHCPHandler(packetData);
                    return;
                }

                NetworkStack.debugger.Send("Content: " + Encoding.ASCII.GetString(udp_packet.UDP_Data));
                UdpClient receiver = UdpClient.Client(udp_packet.DestinationPort);
                if (receiver != null)
                {
                    receiver.receiveData(udp_packet);
                }
            }
            else
            {
                NetworkStack.debugger.Send("But checksum incorrect... Packet Passed.");
            }
        }

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

        public static bool CheckCRC(UDPPacket packet)
        {
            byte[] header = MakeHeader(packet.SourceIP.address, packet.DestinationIP.address, packet.UDP_Length, packet.SourcePort, packet.DestinationPort, packet.UDP_Data);
            UInt16 calculatedcrc = Check(header, 0, header.Length);
            //NetworkStack.debugger.Send("Calculated: 0x" + Utils.Conversion.DecToHex(calculatedcrc));
            //NetworkStack.debugger.Send("Received:  0x" + Utils.Conversion.DecToHex(packet.udpCRC));
            if (calculatedcrc == packet.udpCRC)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected static ushort Check(byte[] buffer, ushort offset, int length)
        {
            uint crc = 0;

            for (ushort w = offset; w < offset + length; w += 2)
            {
                crc += (ushort)((buffer[w] << 8) | buffer[w + 1]);
            }

            crc = (~((crc & 0xFFFF) + (crc >> 16)));
            return (ushort)crc;

        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new UDPPacket();
        }

        internal UDPPacket()
        {
        }

        public UDPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        public UDPPacket(Address source, Address dest, ushort srcPort, ushort destPort, byte[] data)
            : base((ushort)(data.Length + 8), 17, source, dest, 0x00)
        {
            RawData[DataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            RawData[DataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);
            RawData[DataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((destPort >> 0) & 0xFF);
            UDP_Length = (ushort)(data.Length + 8);

            RawData[DataOffset + 4] = (byte)((UDP_Length >> 8) & 0xFF);
            RawData[DataOffset + 5] = (byte)((UDP_Length >> 0) & 0xFF);

            byte[] header = MakeHeader(source.address, dest.address, UDP_Length, srcPort, destPort, data);
            UInt16 calculatedcrc = Check(header, 0, header.Length);

            RawData[DataOffset + 6] = (byte)((calculatedcrc >> 8) & 0xFF);
            RawData[DataOffset + 7] = (byte)((calculatedcrc >> 0) & 0xFF);

            for (int b = 0; b < data.Length; b++)
            {
                RawData[DataOffset + 8 + b] = data[b];
            }

            initFields();
        }

        protected override void initFields()
        {
            base.initFields();
            SourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
            DestinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
            UDP_Length = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
            udpCRC = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        }

        public ushort DestinationPort { get; private set; }
        public ushort SourcePort { get; private set; }
        public ushort UDP_Length { get; private set; }
        public ushort UDP_DataLength => (ushort)(UDP_Length - 8);

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

        public override string ToString()
        {
            return "UDP Packet Src=" + SourceIP + ":" + SourcePort + "," +
                   "Dest=" + DestinationIP + ":" + DestinationPort + ", DataLen=" + UDP_DataLength;
        }
    }
}
