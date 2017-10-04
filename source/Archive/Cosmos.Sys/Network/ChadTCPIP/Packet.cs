using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network
{
    public abstract class Packet {
        protected byte[] mData;

        // This is a method and not a read only property to "signify" to
        // user that it is not a direct access, but incurs a performance
        // penalty to call
        public byte[] GetData() {
            Conclude();
            return mData;
        }

        /// <summary>
        /// Initializes the Packet with some data and sets a size for the header. This is used to determine the size of the bytearray.
        /// </summary>
        /// <param name="aData">A bytearray used to initialize the size of the packet.</param>
        /// <param name="aHeaderSize">Used to determine the difference between the header and the body.</param>
        /// <returns>Always 0</returns>
        protected int Initialize(byte[] aData, int aHeaderSize) {
            mData = new byte[aData.Length + aHeaderSize];
            aData.CopyTo(mData, aHeaderSize);
            return 0;
        }

        /// <summary>
        /// Concludes the Packet
        /// </summary>
        protected virtual void Conclude() {
        }
    }
}
