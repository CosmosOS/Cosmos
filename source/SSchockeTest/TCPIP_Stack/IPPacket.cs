using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
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
        protected IPv4Address sourceIP;
        protected IPv4Address destIP;
        protected UInt16 dataOffset;

        public IPPacket(byte[] rawData)
            : base(rawData)
        {
            ipVersion = (byte)((rawData[14] & 0xF0) >> 4);
            ipHeaderLength = (byte)(rawData[14] & 0x0F);
            tos = rawData[15];
            ipLength = (UInt16)((rawData[16] << 8) | rawData[17]);
            fragmentID = (UInt16)((rawData[18] << 8) | rawData[19]);
            flags = (byte)((rawData[20] & 0xE0) >> 5);
            fragmentOffset = (UInt16)(((rawData[20] & 0x1F) << 8) | rawData[21]);
            ttl = rawData[22];
            proto = rawData[23];
            ipCRC = (UInt16)((rawData[24] << 8) | rawData[25]);
            sourceIP = new IPv4Address(rawData, 26);
            destIP = new IPv4Address(rawData, 30);
            dataOffset = (UInt16)(14 + HeaderLength);
        }

        protected IPPacket(HW.Network.MACAddress srcMAC, HW.Network.MACAddress destMAC, UInt16 dataLength, byte protocol,
            IPv4Address source, IPv4Address dest)
            : base(destMAC, srcMAC, 0x0800, dataLength + 14 + 20)
        {
            mRawData[14] = 0x45;
            mRawData[15] = 0;
            ipLength = (UInt16)(dataLength + 20);
            ipHeaderLength = 5;
            mRawData[16] = (byte)((ipLength >> 8) & 0xFF);
            mRawData[17] = (byte)((ipLength >> 0) & 0xFF);
            fragmentID = TCPIP.NextIPFragmentID();
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
            dataOffset = 34;
        }

        protected UInt16 CalcOcCRC(UInt16 offset, UInt16 length)
        {
            UInt32 crc = 0;

            for (UInt16 w = offset; w < offset+length; w += 2)
            {
                crc += (UInt16)((mRawData[w] << 8) | mRawData[w + 1]);
            }

            crc = (~((crc & 0xFFFF) + (crc >> 16)));

            return (UInt16)crc;
        }

        protected UInt16 CalcIPCRC(UInt16 headerLength)
        {
            return CalcOcCRC(14, headerLength);
        }

        public byte IPVersion
        {
            get { return this.ipVersion; }
        }
        public UInt16 HeaderLength
        {
            get { return (UInt16)(this.ipHeaderLength * 4); }
        }
        public byte TypeOfService
        {
            get { return this.tos; }
        }
        public UInt16 IPLength
        {
            get { return this.ipLength; }
        }
        public UInt16 FragmentID
        {
            get { return this.fragmentID; }
        }
        public UInt16 FragmentOffset
        {
            get { return this.fragmentOffset; }
        }
        public byte Flags
        {
            get { return this.flags; }
        }
        public byte TTL
        {
            get { return this.ttl; }
        }
        public byte Protocol
        {
            get { return this.proto; }
        }
        public UInt16 IPCRC
        {
            get { return this.ipCRC; }
        }
        public IPv4Address SourceIP
        {
            get { return this.sourceIP; }
        }
        public IPv4Address DestinationIP
        {
            get { return this.destIP; }
        }
        public UInt16 DataLength
        {
            get { return (UInt16)(this.ipLength - this.HeaderLength); }
        }
    }
}
