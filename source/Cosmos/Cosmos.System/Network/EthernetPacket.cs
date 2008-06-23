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
        }
    }
}
