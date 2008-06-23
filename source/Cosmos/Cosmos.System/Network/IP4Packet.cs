using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    // http://en.wikipedia.org/wiki/IPv4
    public abstract class IP4Packet : Packet {
        protected byte mHeaderSize = 20;
        protected int mHeaderBegin;

        protected int Initialize(byte[] aData, int aHeaderSize, byte aProtocol, uint aSrcIP, uint aDestIP) {
            mHeaderBegin = base.Initialize(aData, mHeaderSize + aHeaderSize);
            // Version + Header length
            // 0x40 is the version (4 in high nibble, i.e. 4x)
            // Length is # of 32 bit words in header
            mData[mHeaderBegin] = (byte)(0x40 | (mHeaderSize >> 2));
            // Differentiated Services Field
            mData[mHeaderBegin + 1] = 0x00;
            // Total Length - IP Packet + sub packets + data (i.e. IP + UDP + UPD Data)
            int xSize = mData.Length - mHeaderBegin;
            mData[mHeaderBegin + 2] = (byte)(xSize >> 8);
            mData[mHeaderBegin + 3] = (byte)(xSize & 0xFF);
            // IP - Identification - Varies
            // This field is an identification field and is primarily used for uniquely identifying fragments of an original IP datagram. Some experimental work has suggested using the ID field for other purposes, such as for adding packet-tracing information to datagrams in order to help trace back datagrams with spoofed source addresses.
            mData[mHeaderBegin + 4] = 0x5a;
            mData[mHeaderBegin + 5] = 0xf8;
            // Flags + Fragment Offset
            mData[mHeaderBegin + 6] = 0x00;
            mData[mHeaderBegin + 7] = 0x00;
            // TTL
            mData[mHeaderBegin + 8] = 0x80;
            // Protocol
            mData[mHeaderBegin + 9] = aProtocol;
            // IP - Header Checksum - Varies
            // In 1's complement, there are 2 representations for zero: 0000000000000000 and 1111111111111111. 
            // Note that flipping the bits of the one gives you the other. A header checksum of "0"
            // (allowed for some protocols, e.g. UDP) denotes that the checksum was not calculated. 
            // Thus, implementations which do calculate a checksum make sure to give a result of 0xffff rather 
            // that 0, when the checksum is actually zero.
            mData[mHeaderBegin + 10] = 0x09;
            mData[mHeaderBegin + 11] = 0x23;
            // Source IP
            mData[mHeaderBegin + 12] = (byte)(aSrcIP >> 24);
            mData[mHeaderBegin + 13] = (byte)(aSrcIP >> 16);
            mData[mHeaderBegin + 14] = (byte)(aSrcIP >> 8);
            mData[mHeaderBegin + 15] = (byte)(aSrcIP & 0xFF);
            // Destination IP
            mData[mHeaderBegin + 16] = (byte)(aDestIP >> 24);
            mData[mHeaderBegin + 17] = (byte)(aDestIP >> 16);
            mData[mHeaderBegin + 18] = (byte)(aDestIP >> 8);
            mData[mHeaderBegin + 19] = (byte)(aDestIP & 0xFF);

            return mHeaderBegin + mHeaderSize;
        }
    }
}
