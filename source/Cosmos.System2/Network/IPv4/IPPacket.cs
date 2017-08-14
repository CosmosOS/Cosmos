using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL.Network;
using Cosmos.System.Network.ARP;

namespace Cosmos.System.Network.IPv4
{
    public class IPPacket : EthernetPacket
    {
        protected byte ipVersion;
        protected byte ipHeaderLength;
        protected byte tos;
        protected UInt16 ipLength;
        protected UInt16 fragmentID;
        protected UInt16 fragmentOffset;
        protected byte flags;
        protected byte ttl;
        protected byte proto;
        protected UInt16 ipCRC;
        protected Address sourceIP;
        protected Address destIP;
        protected UInt16 dataOffset;

        private static UInt16 sNextFragmentID;

        internal static void IPv4Handler(byte[] packetData)
        {
            IPPacket ip_packet = new IPPacket(packetData);
            //Sys.Console.WriteLine("Received IP Packet");
            //Sys.Console.WriteLine(ip_packet.ToString());
            if (ip_packet.SourceIP == null)
            {
                global::System.Console.WriteLine("SourceIP null in IPv4Handler!");
            }
            ARPCache.Update(ip_packet.SourceIP, ip_packet.SourceMAC);

            if ((NetworkStack.AddressMap.ContainsKey(ip_packet.DestinationIP.Hash) == true) ||
                (ip_packet.DestinationIP.address[3] == 255))
            {
                switch (ip_packet.Protocol)
                {
                    case 1:
                        ICMPPacket.ICMPHandler(packetData);
                        break;
                    //case 6:
                    //    IPv4_TCPHandler(packetData);
                    //    break;
                    case 17:
                        UDPPacket.UDPHandler(packetData);
                        break;
                }
            }
        }

        public static UInt16 NextIPFragmentID
        {
            get
            {
                return sNextFragmentID++;
            }
        }

        internal IPPacket()
            : base()
        { }

        internal IPPacket(byte[] rawData)
            : base(rawData)
        { }

        protected override void initFields()
        {
            base.initFields();
            ipVersion = (byte)((mRawData[14] & 0xF0) >> 4);
            ipHeaderLength = (byte)(mRawData[14] & 0x0F);
            tos = mRawData[15];
            ipLength = (UInt16)((mRawData[16] << 8) | mRawData[17]);
            fragmentID = (UInt16)((mRawData[18] << 8) | mRawData[19]);
            flags = (byte)((mRawData[20] & 0xE0) >> 5);
            fragmentOffset = (UInt16)(((mRawData[20] & 0x1F) << 8) | mRawData[21]);
            ttl = mRawData[22];
            proto = mRawData[23];
            ipCRC = (UInt16)((mRawData[24] << 8) | mRawData[25]);
            sourceIP = new Address(mRawData, 26);
            destIP = new Address(mRawData, 30);
            dataOffset = (UInt16)(14 + HeaderLength);
        }

        protected IPPacket(UInt16 dataLength, byte protocol, Address source, Address dest)
            : this(MACAddress.None, MACAddress.None, dataLength, protocol, source, dest)
        { }

        private IPPacket(MACAddress srcMAC, MACAddress destMAC, UInt16 dataLength, byte protocol,
            Address source, Address dest)
            : base(destMAC, srcMAC, 0x0800, dataLength + 14 + 20)
        {
            mRawData[14] = 0x45;
            mRawData[15] = 0;
            ipLength = (UInt16)(dataLength + 20);
            ipHeaderLength = 5;
            mRawData[16] = (byte)((ipLength >> 8) & 0xFF);
            mRawData[17] = (byte)((ipLength >> 0) & 0xFF);
            fragmentID = NextIPFragmentID;
            mRawData[18] = (byte)((fragmentID >> 8) & 0xFF);
            mRawData[19] = (byte)((fragmentID >> 0) & 0xFF);
            mRawData[20] = 0x00;
            mRawData[21] = 0x00;
            mRawData[22] = 0x80;
            mRawData[23] = protocol;
            mRawData[24] = 0x00;
            mRawData[25] = 0x00;
            for (int b = 0; b < 4; b++)
            {
                mRawData[26 + b] = source.address[b];
                mRawData[30 + b] = dest.address[b];
            }
            ipCRC = CalcIPCRC(20);
            mRawData[24] = (byte)((ipCRC >> 8) & 0xFF);
            mRawData[25] = (byte)((ipCRC >> 0) & 0xFF);

            initFields();
        }

        protected UInt16 CalcOcCRC(UInt16 offset, UInt16 length)
        {
            return IPPacket.CalcOcCRC(this.RawData, offset, length);
        }

        protected static UInt16 CalcOcCRC(byte[] buffer, UInt16 offset, int length)
        {
            UInt32 crc = 0;

            for (UInt16 w = offset; w < offset + length; w += 2)
            {
                crc += (UInt16)((buffer[w] << 8) | buffer[w + 1]);
            }

            crc = (~((crc & 0xFFFF) + (crc >> 16)));

            return (UInt16)crc;
        }

        protected UInt16 CalcIPCRC(UInt16 headerLength)
        {
            return CalcOcCRC(14, headerLength);
        }

        internal byte IPVersion
        {
            get { return this.ipVersion; }
        }
        internal UInt16 HeaderLength
        {
            get { return (UInt16)(this.ipHeaderLength * 4); }
        }
        internal byte TypeOfService
        {
            get { return this.tos; }
        }
        internal UInt16 IPLength
        {
            get { return this.ipLength; }
        }
        internal UInt16 FragmentID
        {
            get { return this.fragmentID; }
        }
        internal UInt16 FragmentOffset
        {
            get { return this.fragmentOffset; }
        }
        internal byte Flags
        {
            get { return this.flags; }
        }
        internal byte TTL
        {
            get { return this.ttl; }
        }
        internal byte Protocol
        {
            get { return this.proto; }
        }
        internal UInt16 IPCRC
        {
            get { return this.ipCRC; }
        }
        internal Address SourceIP
        {
            get { return this.sourceIP; }
        }
        internal Address DestinationIP
        {
            get { return this.destIP; }
        }
        internal UInt16 DataLength
        {
            get { return (UInt16)(this.ipLength - this.HeaderLength); }
        }

        public override string ToString()
        {
            return "IP Packet Src=" + sourceIP + ", Dest=" + destIP + ", Protocol=" + proto + ", TTL=" + ttl + ", DataLen=" + DataLength;
        }
    }
}
