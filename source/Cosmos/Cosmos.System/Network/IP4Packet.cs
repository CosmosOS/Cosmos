using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    public abstract class IP4Packet : Packet {
        protected int mHeaderSize = 20;
        protected int mHeaderBegin;

        protected override int Initialize(byte[] aData, int aHeaderSize) {
           mHeaderBegin = base.Initialize(aData, mHeaderSize + aHeaderSize);
           return mHeaderBegin;
        }
    }
}
