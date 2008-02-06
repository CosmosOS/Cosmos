using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target=typeof(Int64))]
    public class Int64Impl
    {
        public static string ToString(ref uint aThis)
        {
            string xDigits = "0123456789ABCDEF";
            char[] xResult = new char[10];
            xResult[0] = '0';
            xResult[1] = 'x';
            xResult[2] = xDigits[(int)(aThis >> 28) & 0xF];
            xResult[3] = xDigits[(int)(aThis >> 24) & 0xF];
            xResult[4] = xDigits[(int)(aThis >> 20) & 0xF];
            xResult[5] = xDigits[(int)(aThis >> 16) & 0xF];
            xResult[6] = xDigits[(int)(aThis >> 12) & 0xF];
            xResult[7] = xDigits[(int)(aThis >> 8) & 0xF];
            xResult[8] = xDigits[(int)(aThis >> 4) & 0xF];
            xResult[9] = xDigits[(int)aThis & 0xF];
            return new String(xResult);
        }
    }
}
