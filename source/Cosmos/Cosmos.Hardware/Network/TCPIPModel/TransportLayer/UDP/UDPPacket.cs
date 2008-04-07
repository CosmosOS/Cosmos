using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Network.TCPIPModel.TransportLayer.UDP
{
    public class UDPPacket
    {
        public UDPPacket(byte[] data)
        {
            throw new NotImplementedException("UDP not impl");
        }

        public UInt16 SourcePort { get; set; }

        public UInt16 DestinationPort { get; set; }

        public UInt16 Length { get; set; }

        public UInt16 Checksum { get; set; }

        public UInt32 Data { get; set; }
    }
}
