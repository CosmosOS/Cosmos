using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel {
	public static class Heap {
		private static uint mStartAddress = 4 * 1024 * 1024;
		private static uint mCurrentAddress = mStartAddress;
		private static uint mLength = (32 * 1024 * 1024) - mStartAddress;
		private const uint DefaultStartAddress = 4 * 1024 * 1024;
		private const uint DefaultMaxMemory = 32 * 1024 * 1024;

		//		public static void Initialize(uint aStartAddress, uint aLength) {
		//			mStartAddress = aStartAddress;
		//			mCurrentAddress = aStartAddress;
		//			mLength = aLength;
		//			//DebugUtil.SendMM_Init(aStartAddress, aLength);
		//		}

		private static void CheckInit() {
			if (mStartAddress == 0) {
				mCurrentAddress = mStartAddress = DefaultStartAddress;
				mLength = mCurrentAddress - DefaultMaxMemory;
			}
		}

		[GlueMethod(Type = GlueMethodType.Heap_Alloc)]
		public static uint MemAlloc(uint aLength) {
			CheckInit();
			DebugUtil.SendMM_Alloc(mCurrentAddress, aLength);
			uint xResult = mCurrentAddress;
			mCurrentAddress += aLength;
			if (mCurrentAddress >= (mStartAddress + mLength)) {
				DebugUtil.SendError("MM", "Reached maximum memory");
			}
			return xResult;
		}

		[GlueMethod(Type = GlueMethodType.Heap_Free)]
		public static void MemFree(uint aPointer) {
			// todo: implement memory freeing
		}
	}
}
