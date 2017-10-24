using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System2.Encoding
{
    public abstract class CosmosEncoding
    {
        public abstract Byte[] GetBytes(String s);
        public abstract String GetString(Byte[] bytes);
        public abstract int GetMaxByteCount(int ByteCount);
    }
}
