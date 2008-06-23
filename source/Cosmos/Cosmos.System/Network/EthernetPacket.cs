using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    public class EthernetPacket : Packet {
        protected byte mHeaderSize = 14;
        protected int mHeaderBegin;

        public EthernetPacket(byte[] aData) {
            mHeaderBegin = Initialize(aData, mHeaderSize);
            // Ethernet - Destination
            mData[0] = 0xFF;
            mData[1] = 0xFF;
            mData[2] = 0xFF;
            mData[3] = 0xFF;
            mData[4] = 0xFF;
            mData[5] = 0xFF;
            // Ethernet - Source
            mData[6] = 0x00;
            mData[7] = 0x50;
            mData[8] = 0x56;
            mData[9] = 0x22;
            mData[10] = 0x22;
            mData[11] = 0x0d;
            // Ethernet - Type - 0800 = IP
            mData[12] = 0x08;
            mData[13] = 0x00;
        }
    }
}
