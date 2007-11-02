using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU.CustomImplementation.System {
	public static class StringImpl {
		[MethodAlias(Name = "System.String System.String.FastAllocateString(System.Int32)")]
		public static String FastAllocateString(int aLength) {
			Char[] xItems = new Char[aLength];
			return new String(xItems);
		}

		public static unsafe uint* GetStorageMetal(uint* aStringPtr) {
			return aStringPtr + 1;
		}

		public static unsafe uint GetStorage(uint aStringPtr) {
			return aStringPtr += 12;
		}

		public static unsafe uint* GetStorageNormal(uint* aStringPtr) {
			uint* xResult = aStringPtr;
			xResult = xResult + 3;
			xResult = (uint*)(*xResult);
			xResult += 3;
			return xResult;
		}
	}
}
