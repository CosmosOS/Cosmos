using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frotz.Other
{
    using zbyte = System.Byte;
    using zword = System.UInt16;
    using zlong = System.UInt32;

    public static class ZMath
    {
        public static zlong MakeInt(char a, char b, char c, char d)
        {
            return MakeInt((byte)a, (byte)b, (byte)c, (byte)d);
        }

        public static zlong MakeInt(byte a, byte b, byte c, byte d)
        {
            return ((zlong)(((a) << 24) | ((b) << 16) | ((c) << 8) | (d)));
        }

        internal static void clearArray(byte[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = 0;
            }
        }
    }
}
