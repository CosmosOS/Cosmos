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

		[MethodAlias(Name = "System.Void System.String..ctor(System.Char[],System.Int32,System.Int32)")]
		public static void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength) {
			Char[] newChars = new Char[aLength];
			Array.Copy(aChars, aStartIndex, newChars, 0, aLength);
			aStorage = newChars;
		}

		[MethodAlias(Name = "System.Void System.String..ctor(System.Char[])")]
		public static void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")] ref Char[] aStorage, Char[] aChars) {
			aStorage = aChars;
		}

		public static unsafe uint* GetStorageMetal(uint* aStringPtr) {
			return aStringPtr + 1;
		}

		public static unsafe uint* GetStorage(string aStringPtr) {
//			uint* thePtr = &aStringPtr;
//			thePtr += 3;
//			//return aStringPtr += 12;
//			return (uint)thePtr;
			return (uint*)0;
		}

		public static unsafe uint GetStorage_Impl(uint* aStringPtr) {
			uint* xThis = aStringPtr;
			xThis += 3;
			xThis = (uint*)*xThis;
			xThis += 3;
			return (uint)xThis;
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
