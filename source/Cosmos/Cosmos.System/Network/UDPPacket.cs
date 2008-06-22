using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    // http://en.wikipedia.org/wiki/User_Datagram_Protocol
    public class UDPPacket : Packet {
        public UDPPacket(int aSrcPort, int aDestPort, byte[] aData) {
            Initialize(aData, 8);
            // Source Port
            mData[0] = (byte)(aSrcPort >> 8);
            mData[1] = (byte)(aSrcPort & 0xFF);
            // Destination Port
            mData[2] = (byte)(aDestPort >> 8);
            mData[3] = (byte)(aDestPort & 0xFF);
            // Length
            mData[4] = (byte)(mData.Length >> 8);
            mData[5] = (byte)(mData.Length & 0xFF);
        }

        protected override void Finalize() {
            base.Finalize();
            // Checksum
            //TODO: Uses info from IPHeader to create check sum as well
            mData[6] = 0;
            mData[7] = 0;
        }
    }
}
