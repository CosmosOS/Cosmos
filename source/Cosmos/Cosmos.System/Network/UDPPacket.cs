using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    // http://en.wikipedia.org/wiki/User_Datagram_Protocol
    public class UDPPacket : IP4Packet {
        public UDPPacket(int aSrcPort, int aDestPort, byte[] aData) {
            mHeaderBegin = Initialize(aData, 8);
            // Source Port
            mData[mHeaderBegin] = (byte)(aSrcPort >> 8);
            mData[mHeaderBegin + 1] = (byte)(aSrcPort & 0xFF);
            // Destination Port
            mData[mHeaderBegin + 2] = (byte)(aDestPort >> 8);
            mData[mHeaderBegin + 3] = (byte)(aDestPort & 0xFF);
            // Length
            mData[mHeaderBegin + 4] = (byte)(mData.Length >> 8);
            mData[mHeaderBegin + 5] = (byte)(mData.Length & 0xFF);
        }

        protected new int mHeaderBegin = 0;

        protected override void Conclude() {
            base.Conclude();
            // Checksum
            //TODO: Uses info from IPHeader to create check sum as well
            mData[mHeaderBegin + 6] = 0;
            mData[mHeaderBegin + 7] = 0;
        }
    }
}
