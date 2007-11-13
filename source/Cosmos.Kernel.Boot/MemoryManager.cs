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
			DebugUtil.SendMM_Init(aStartAddress, aLength);
		}

		[GlueMethod(MethodType = GlueMethodTypeEnum.Heap_MemAlloc)]
		public static uint MemAlloc(uint aLength) {
			DebugUtil.SendMM_Alloc(mCurrentAddress, aLength);
			uint xResult = mCurrentAddress;
			mCurrentAddress += aLength;
			if (mCurrentAddress >= (mStartAddress + mLength)) {
				DebugUtil.SendError("MM", "Reached maximum memory");
			}
			return xResult;
		}
	}
}