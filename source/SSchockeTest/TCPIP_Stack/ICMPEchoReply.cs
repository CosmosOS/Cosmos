using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
{
    public class ICMPEchoReply : ICMPPacket
    {
        public ICMPEchoReply(ICMPEchoRequest request)
            : base(request.DestinationMAC, request.SourceMAC, request.DestinationIP, request.SourceIP, 0, 0,
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
    }
}
