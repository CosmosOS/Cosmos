using System;

namespace Cosmos.Sys.Network.TCPIP.ICMP
{
    internal class ICMPEchoReply : ICMPPacket
    {
        protected UInt16 icmpID;
        protected UInt16 icmpSequence;

        internal ICMPEchoReply(byte[] rawData)
            : base(rawData)
        {}

        protected override void initFields()
        {
            base.initFields();
            icmpID = (UInt16)((mRawData[this.dataOffset + 4] << 8) | mRawData[this.dataOffset + 5]);
            icmpSequence = (UInt16)((mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
        }

        internal ICMPEchoReply(ICMPEchoRequest request)
            : base(request.DestinationIP, request.SourceIP, 0, 0,
                    request.ICMP_ID, request.ICMP_Sequence, (UInt16)(request.ICMP_DataLength + 8))
        {
            for (int b = 0; b < this.ICMP_DataLength; b++)
            {
                mRawData[this.dataOffset + 8 + b] = request.RawData[this.dataOffset + 8 + b];
            }

            mRawData[this.dataOffset + 2] = 0x00;
            mRawData[this.dataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((UInt16)(this.ICMP_DataLength + 8));
            mRawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
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
            return "ICMP Echo Reply Src=" + sourceIP + ", Dest=" + destIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
}
