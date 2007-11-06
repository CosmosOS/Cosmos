using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class MemoryManager {
		private static uint mStartAddress;
		private static uint mCurrentAddress;
		private static uint mLength;
		public static void Initialize(uint aStartAddress, uint aLength) {
			mStartAddress = aStartAddress;
			mCurrentAddress = aStartAddress;
			mLength = aLength;
			DebugUtil.Write("Initializing MemoryManager. Start Address = ");
			IO.WriteSerialHexNumber(0, aStartAddress);
			DebugUtil.Write(", Length = ");
			IO.WriteSerialHexNumber(0, aLength);
			DebugUtil.WriteLine("");
		}

		[GlueMethod(MethodType = GlueMethodTypeEnum.Heap_MemAlloc)]
		public static uint MemAlloc(uint aLength) {
			Debug.Write("MemAlloc (aLength = ");
			IO.WriteSerialHexNumber(0, aLength);
			Debug.WriteLine(")");
			Debug.Write("    CurrentAddress = ");
			IO.WriteSerialHexNumber(0, mCurrentAddress);
			Debug.WriteLine(")");
			uint xResult = mCurrentAddress;
			mCurrentAddress += aLength;
			Debug.Write("    NewCurrentAddress = ");
			IO.WriteSerialHexNumber(0, mCurrentAddress);
			Debug.WriteLine(")");
			uint xMaxAddr = (mStartAddress + mLength);
			Debug.Write("    MaxAddr = ");
			IO.WriteSerialHexNumber(0, xMaxAddr);
			Debug.WriteLine(")");
			if (mCurrentAddress >= (mStartAddress + mLength)) {
				DebugUtil.WriteLine("ERROR: Reached maximum memory");
			}
			return xResult;
		}
	}
}