using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
{
    public class ARPRequest_EthernetIPv4 : ARPPacket_EthernetIPv4
    {
        public ARPRequest_EthernetIPv4(byte[] rawData)
            : base(rawData)
        {}
    }
}
