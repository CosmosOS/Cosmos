using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target=typeof(UInt32))]
	public static class UInt32Impl {
		public static string ToString(ref uint aThis) {
			return GetNumberString(aThis, false);
		}

		public static string GetNumberString(uint aValue, bool aIsNegative) {
			if (aValue == 0) {
				if (aIsNegative) {
					return "-0";
				} else {
					return "0";
				}
			}
			const string xDigits = "0123456789";
			char[] xResultChars = new char[11];
			int xCurrentPos = 10;
			while (aValue > 0) {
				byte xPos = (byte)(aValue % 10);
				aValue /= 10;
				xResultChars[xCurrentPos] = xDigits[xPos];
				xCurrentPos -= 1;
			}
			if (aIsNegative) {
				xResultChars[xCurrentPos] = '-';
				xCurrentPos -= 1;
			}
			return new String(xResultChars, xCurrentPos + 1, 10 - xCurrentPos);
		}
	}
}
