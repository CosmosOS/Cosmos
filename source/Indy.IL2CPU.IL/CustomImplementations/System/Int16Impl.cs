using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target=typeof(short))]
	public static class Int16Impl {
		public static string ToString(short aThis) {
			string xDigits = "0123456789ABCDEF";
			char[] xResult = new char[6];
			xResult[0] = '0';
			xResult[1] = 'x';
			xResult[2] = xDigits[aThis >> 12];
			xResult[3] = xDigits[aThis >> 8];
			xResult[4] = xDigits[aThis >> 4];
			xResult[5] = xDigits[aThis & 0xF];
			return new String(xResult);
		}

        public static Int16 Parse(string s)
        {
            const string digits = "0123456789";
            Int16 result = 0;

            for (int i = 0; i < s.Length; i++)
            {
                Int16 ind = (Int16)digits.IndexOf(digits[i]);
				result = (short)((result * 10) + ind);
            }

            return result;
        }
    }
}
