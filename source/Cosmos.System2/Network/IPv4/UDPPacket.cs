using System;
using Sys = System;

namespace Cosmos.System.Network.IPv4
{
    public class UDPPacket : IPPacket
    {
        protected UInt16 sourcePort;
        protected UInt16 destPort;
        protected UInt16 udpLen;
        protected UInt16 udpCRC;

        internal static void UDPHandler(byte[] packetData)
        {
            UDPPacket udp_packet = new UDPPacket(packetData);
            Sys.Console.WriteLine("Received UDP packet from " + udp_packet.SourceIP.ToString() + ":" + udp_packet.SourcePort.ToString());
            UdpClient receiver = UdpClient.Client(udp_packet.DestinationPort);
            if (receiver != null)
            {
                Sys.Console.WriteLine("UDP Packet is for registered client");
                receiver.receiveData(udp_packet);
            //    DataReceived dlgt = udpClients[udp_packet.DestinationPort];
            //    if (dlgt != null)
            //    {
            //        dlgt(new IPv4EndPoint(udp_packet.SourceIP, udp_packet.SourcePort), udp_packet.UDP_Data);
            //    }
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new UDPPacket();
        }

        internal UDPPacket()
            : base()
        { }

        public UDPPacket(byte[] rawData)
            : base(rawData)
        {}

        public UDPPacket(Address source, Address dest, UInt16 srcPort, UInt16 destPort, byte[] data)
            : base((UInt16)(data.Length + 8), 17, source, dest)
        {
            mRawData[this.dataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);
            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);
            udpLen = (UInt16)(data.Length + 8);
            mRawData[this.dataOffset + 4] = (byte)((udpLen >> 8) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((udpLen >> 0) & 0xFF);
            mRawData[this.dataOffset + 6] = 0;
            mRawData[this.dataOffset + 7] = 0;
            for (int b = 0; b < data.Length; b++)
            {
                mRawData[this.dataOffset + 8 + b] = data[b];
            }

            initFields();
        }

        protected override void initFields()
        {
            base.initFields();
            sourcePort = (UInt16)((mRawData[this.dataOffset] << 8) | mRawData[this.dataOffset + 1]);
            destPort = (UInt16)((mRawData[this.dataOffset + 2] << 8) | mRawData[this.dataOffset + 3]);
            udpLen = (UInt16)((mRawData[this.dataOffset + 4] << 8) | mRawData[this.dataOffset + 5]);
            udpCRC = (UInt16)((mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
        }

        internal UInt16 DestinationPort
        {
            get { return this.destPort; }
        }
        internal UInt16 SourcePort
        {
            get { return this.sourcePort; }
        }
        internal UInt16 UDP_Length
        {
            get { return this.udpLen; }
        }
        internal UInt16 UDP_DataLength
        {
            get { return (UInt16)(this.udpLen - 8); }
        }
        internal byte[] UDP_Data
        {
            get
            {
                byte[] data = new byte[this.udpLen - 8];

                for (int b = 0; b < data.Length; b++)
                {
                    data[b] = this.mRawData[this.dataOffset + 8 + b];
                }

                return data;
            }
        }

        public override string ToString()
        {
            return "UDP Packet Src=" + sourceIP + ":" + sourcePort + ", Dest=" + destIP + ":" + destPort + ", DataLen=" + UDP_DataLength;
        }
    }
}
