using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    public abstract class Packet {
        protected byte[] mData;

        // This is a method and not a read only property to "signify" to
        // user that it is not a direct access, but incurs a performance
        // penalty to call
        public byte[] GetData() {
            Finalize();
            return mData;
        }

        protected void Initialize(byte[] aData, int aHeaderSize) {
            mData = new byte[aData.Length + 8];
            aData.CopyTo(mData, 8);
        }

        protected virtual void Finalize() {
        }
    }
}
