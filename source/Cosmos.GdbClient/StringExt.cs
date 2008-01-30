using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.GdbClient
{
    static class StringExt
    {
        public static byte[] GetBytes(this string hex)
        {
            byte[] data = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                data[i] = byte.Parse(hex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return data;
        }
    }
}
