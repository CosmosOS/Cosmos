﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(UInt64))]
	public class UInt64Impl {
		public static string ToString(ref ulong aThis) {
      return UInt64Impl2.GetNumberString(aThis, false);
		}
  }

  // See note in UInt32Impl
  public class UInt64Impl2 {
    public static string GetNumberString(ulong aValue, bool aIsNegative) {
			const string xDigits = "0123456789";
			char[] xResultChars = new char[21];
			int xCurrentPos = 20;
            if (aValue > 0)
            {
                while (aValue > 0)
                {
                    byte xPos = (byte)(aValue % 10);
                    aValue /= 10;
                    xResultChars[xCurrentPos] = xDigits[xPos];
                    xCurrentPos -= 1;
                }
            }
            else {
                xResultChars[xCurrentPos] = '0';
                xCurrentPos -= 1;
            }
		    if (aIsNegative) {
				xResultChars[xCurrentPos] = '-';
				xCurrentPos -= 1;
			}
			return new String(xResultChars, xCurrentPos + 1, 20 - xCurrentPos);
		}
	}
}
