using System;

namespace Cosmos.Sys.Network.TCPIP.ICMP
{
    internal class ICMPEchoRequest : ICMPPacket
    {
        protected UInt16 icmpID;
        protected UInt16 icmpSequence;

        internal ICMPEchoRequest(byte[] rawData)
            : base(rawData)
        {}

        internal ICMPEchoRequest(IPv4Address source, IPv4Address dest, UInt16 id, UInt16 sequence)
            : base(source, dest, 8, 0, id, sequence, 40)
        {
            for (byte b = 8; b < this.ICMP_DataLength; b++)
            {
                mRawData[this.dataOffset + b] = b;
            }

            mRawData[this.dataOffset + 2] = 0x00;
            mRawData[this.dataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((UInt16)(this.ICMP_DataLength + 8));
            mRawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        protected override void initFields()
        {
            base.initFields();
            icmpID = (UInt16)((mRawData[this.dataOffset + 4] << 8) | mRawData[this.dataOffset + 5]);
            icmpSequence = (UInt16)((mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
        }

        internal UInt16 ICMP_ID
        {
            get { return this.icmpID; }
        }
        internal UInt16 ICMP_Sequence
        {
            get { return this.icmpSequence; }
        }

        public override string ToString()
        {
            return "ICMP Echo Request Src=" + sourceIP + ", Dest=" + destIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
}
