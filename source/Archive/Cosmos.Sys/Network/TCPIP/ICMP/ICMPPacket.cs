using System;

namespace Cosmos.Sys.Network.TCPIP.ICMP
{
    internal class ICMPPacket : IPPacket
    {
        protected byte icmpType;
        protected byte icmpCode;
        protected UInt16 icmpCRC;

        public ICMPPacket(byte[] rawData)
            : base(rawData)
        {}

        protected override void initFields()
        {
            base.initFields();
            icmpType = mRawData[this.dataOffset];
            icmpCode = mRawData[this.dataOffset + 1];
            icmpCRC = (UInt16)((mRawData[this.dataOffset + 2] << 8) | mRawData[this.dataOffset + 3]);
        }

        internal ICMPPacket(IPv4Address source, IPv4Address dest, byte type, byte code, UInt16 id, UInt16 seq, UInt16 icmpDataSize)
            : base(icmpDataSize, 1, source, dest)
        {
            mRawData[this.dataOffset] = type;
            mRawData[this.dataOffset + 1] = code;
            mRawData[this.dataOffset + 2] = 0x00;
            mRawData[this.dataOffset + 3] = 0x00;
            mRawData[this.dataOffset + 4] = (byte)((id >> 8) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((id >> 0) & 0xFF);
            mRawData[this.dataOffset + 6] = (byte)((seq >> 8) & 0xFF);
            mRawData[this.dataOffset + 7] = (byte)((seq >> 0) & 0xFF);

            icmpCRC = CalcICMPCRC((UInt16)(icmpDataSize + 8));
            mRawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
            initFields();
        }

        protected UInt16 CalcICMPCRC(UInt16 length)
        {
            return CalcOcCRC(this.dataOffset, length);
        }

        internal byte ICMP_Type
        {
            get { return this.icmpType; }
        }
        internal byte ICMP_Code
        {
            get { return this.icmpCode; }
        }
        internal UInt16 ICMP_CRC
        {
            get { return this.icmpCRC; }
        }
        internal UInt16 ICMP_DataLength
        {
            get { return (UInt16)(this.DataLength - 8); }
        }

        internal byte[] GetICMPData()
        {
            byte[] data = new byte[ICMP_DataLength];

            for (int b = 0; b < ICMP_DataLength; b++)
            {
                data[b] = mRawData[this.dataOffset + 8 + b];
            }

            return data;
        }

        public override string ToString()
        {
            return "ICMP Packet Src=" + sourceIP + ", Dest=" + destIP + ", Type=" + icmpType + ", Code=" + icmpCode;
        }
    }
}
