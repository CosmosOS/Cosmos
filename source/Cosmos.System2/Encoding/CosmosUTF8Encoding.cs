#define COSMOSDEBUG
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System2.Encoding
{
    public class CosmosUTF8Encoding : CosmosEncoding
    {
        public override byte[] GetBytes(string s)
        {
            Global.mFileSystemDebugger.SendInternal($"Encoding string {s}");

            //byte[] xResult = new byte[GetMaxByteCount(s.Length)];
            List <byte> xResult = new List<byte>();
            /* Only Ascii for now */
            foreach (var aChar in s)
            {
                Global.mFileSystemDebugger.SendInternal($"Encoding char {aChar}");
                if (aChar > 0 || aChar < 127)
                    xResult.Add((byte)aChar);
                else
                    throw new ArgumentOutOfRangeException("Input string contains invalid characters for UTF-8");
            }

            return xResult.ToArray();
            //throw new NotImplementedException("GetBytes()");
        }

        /* Some UFT-8 char can occupy 3 bytes */
        public override int GetMaxByteCount(int ByteCount) => 3 * ByteCount;

        public override string GetString(byte[] bytes)
        {
            throw new NotImplementedException("GetString()");
        }
    }
}
