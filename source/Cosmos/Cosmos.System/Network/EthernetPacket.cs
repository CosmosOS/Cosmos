using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    public class EthernetPacket : Packet {
        protected byte mHeaderSize = 14;
        protected int mHeaderBegin;

        public EthernetPacket(byte[] aData, UInt64 aSrcMAC, UInt64 aDestMAC, UInt16 aType) {
            mHeaderBegin = Initialize(aData, mHeaderSize);
            // Ethernet - Destination
            mData[0] = (byte)(aDestMAC >> 40);
            mData[1] = (byte)(aDestMAC >> 32);
            mData[2] = (byte)(aDestMAC >> 24);
            mData[3] = (byte)(aDestMAC >> 16);
            mData[4] = (byte)(aDestMAC >> 8);
            mData[5] = (byte)aDestMAC;
            // Ethernet - Source
            mData[6] = (byte)(aSrcMAC >> 40);
            mData[7] = (byte)(aSrcMAC >> 32);
            mData[8] = (byte)(aSrcMAC >> 24);
            mData[9] = (byte)(aSrcMAC >> 16);
            mData[10] = (byte)(aSrcMAC >> 8);
            mData[11] = (byte)aSrcMAC;
            // Ethernet - Type - 0800 = IP
            mData[12] = (byte)(aType >> 8);
            mData[13] = (byte)aType;
        }

    }
}
