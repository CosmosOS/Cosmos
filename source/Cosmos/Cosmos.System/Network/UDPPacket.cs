using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    // http://en.wikipedia.org/wiki/User_Datagram_Protocol
    public class UDPPacket : IP4Packet {
        public UDPPacket(uint aSrcIP, UInt16 aSrcPort, uint aDestIP, UInt16 aDestPort, byte[] aData) {
            mHeaderBegin = Initialize(aData, 8, 0x11, aSrcIP, aDestIP);
            // Source Port
            mData[mHeaderBegin] = (byte)(aSrcPort >> 8);
            mData[mHeaderBegin + 1] = (byte)(aSrcPort & 0xFF);
            // Destination Port
            mData[mHeaderBegin + 2] = (byte)(aDestPort >> 8);
            mData[mHeaderBegin + 3] = (byte)(aDestPort & 0xFF);
            // Length
            int xSize = mData.Length - mHeaderBegin;
            mData[mHeaderBegin + 4] = (byte)(xSize >> 8);
            mData[mHeaderBegin + 5] = (byte)(xSize & 0xFF);
        }

        protected new int mHeaderBegin;

        protected override void Conclude() {
            base.Conclude();
            // Checksum
            //TODO: Uses info from IPHeader to create check sum as well
            mData[mHeaderBegin + 6] = 0;
            mData[mHeaderBegin + 7] = 0;
        }
    }
}
