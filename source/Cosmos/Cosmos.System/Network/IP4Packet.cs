using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    public abstract class IP4Packet : Packet {
        protected int mHeaderSize = 20;
        protected int mHeaderBegin;

        protected int Initialize(byte[] aData, int aHeaderSize, uint aSrcIP, uint aDestIP) {
            mHeaderBegin = base.Initialize(aData, mHeaderSize + aHeaderSize);
            // IP - Version + Header length
            mData[mHeaderBegin] = 0x45;
            // IP - Differentiated Services Field
            mData[mHeaderBegin + 1] = 0x00;
            // IP - Total Length
            mData[mHeaderBegin + 2] = 0x00;
            mData[mHeaderBegin + 3] = 0x1D;
            // IP - Identification - Varies
            // This field is an identification field and is primarily used for uniquely identifying fragments of an original IP datagram. Some experimental work has suggested using the ID field for other purposes, such as for adding packet-tracing information to datagrams in order to help trace back datagrams with spoofed source addresses.
            mData[mHeaderBegin + 4] = 0x5a;
            mData[mHeaderBegin + 5] = 0xf8;
            // IP - Flags + Fragment Offset
            mData[mHeaderBegin + 6] = 0x00;
            mData[mHeaderBegin + 7] = 0x00;
            // IP - TTL
            mData[mHeaderBegin + 8] = 0x80;
            // IP - Protocol, x11 = UDP
            mData[mHeaderBegin + 9] = 0x11;
            // IP - Header Checksum - Varies
            // In 1's complement, there are 2 representations for zero: 0000000000000000 and 1111111111111111. 
            // Note that flipping the bits of the one gives you the other. A header checksum of "0"
            // (allowed for some protocols, e.g. UDP) denotes that the checksum was not calculated. 
            // Thus, implementations which do calculate a checksum make sure to give a result of 0xffff rather 
            // that 0, when the checksum is actually zero.
            mData[mHeaderBegin + 10] = 0x09;
            mData[mHeaderBegin + 11] = 0x23;
            // IP - Source IP
            mData[mHeaderBegin + 12] = 0xc0;
            mData[mHeaderBegin + 13] = 0xa8;
            mData[mHeaderBegin + 14] = 0x16;
            mData[mHeaderBegin + 15] = 0x0d;
            // IP - Destination IP
            mData[mHeaderBegin + 16] = 0xFF;
            mData[mHeaderBegin + 17] = 0xFF;
            mData[mHeaderBegin + 18] = 0xFF;
            mData[mHeaderBegin + 19] = 0xFF;
            return mHeaderBegin + mHeaderSize;
        }
    }
}
