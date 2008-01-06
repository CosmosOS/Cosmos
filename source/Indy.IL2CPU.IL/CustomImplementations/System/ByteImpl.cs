using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(byte))]
	public static class ByteImpl {
		public static string ToString(byte aThis) {
			char[] xResult = new char[4];
			string xDigits = "0123456789ABCDEF";
			xResult[0] = '0';
			xResult[1] = 'x';
			xResult[3] = xDigits[aThis & 0xF];
			xResult[2] = xDigits[aThis >> 4];
			return new String(xResult);
		}
	}
}