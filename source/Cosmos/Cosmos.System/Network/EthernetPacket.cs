using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    public class EthernetPacket : Packet {
        protected byte mHeaderSize = 14;
        protected int mHeaderBegin;

//        public EthernetPacket(byte[] aData, UInt64 aSrcMAC, UInt64 aDestMAC, UInt16 aType) {
        public EthernetPacket(byte[] aData, UInt16 aType) {
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
            mData[12] = (byte)(aType >> 8);
            mData[13] = (byte)aType;
        }

        // TODO: Remove this, this is becuase of a 64 bit bug with Cosmos
        public void SetSrcMAC(byte aMAC1, byte aMAC2, byte aMAC3, byte aMAC4, byte aMAC5, byte aMAC6) {
            mData[6] = aMAC1;
            mData[7] = aMAC2;
            mData[8] = aMAC3;
            mData[9] = aMAC4;
            mData[10] = aMAC5;
            mData[11] = aMAC6;
        }
    }
}
