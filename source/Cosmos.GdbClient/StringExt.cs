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
                data[i / 2] = byte.Parse(hex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return data;
        }

        public static string[] SplitCount(this string source, params byte[] lengths)
        {
            List<string> result = new List<string>();

            int index = 0;

            foreach (byte b in lengths)
            {
                result.Add(source.Substring(index, b));
                index += b;
            }

            return result.ToArray();
        }
    }
}
