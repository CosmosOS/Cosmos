using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
{
    public class ICMPEchoRequest : ICMPPacket
    {
        protected UInt16 icmpID;
        protected UInt16 icmpSequence;

        public ICMPEchoRequest(byte[] rawData)
            : base(rawData)
        {
            icmpID = (UInt16)((rawData[this.dataOffset + 4] << 8) | rawData[this.dataOffset + 5]);
            icmpSequence = (UInt16)((rawData[this.dataOffset + 6] << 8) | rawData[this.dataOffset + 7]);
        }

        public UInt16 ICMP_ID
        {
            get { return this.icmpID; }
        }
        public UInt16 ICMP_Sequence
        {
            get { return this.icmpSequence; }
        } 
    }
}
