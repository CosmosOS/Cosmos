using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel {
	public unsafe static class Heap {
		private enum MemoryBlockState: byte {
			Free,
			Used,
			EndOfMemory
		}
		private unsafe struct MemoryBlock {
			public MemoryBlockState State;
			public MemoryBlock* Next;
			public byte FirstByte;
		}
		private static uint mStart;
		private static uint mStartAddress;
		//		private static uint mCurrentAddress = mStartAddress;
		private static uint mLength;
		private static MemoryBlock* mFirstBlock;
		//private const uint DefaultStartAddress = 4 * 1024 * 1024;
		//private const uint DefaultMaxMemory = 32 * 1024 * 1024;

		private static void ClearMemory(uint aStartAddress, uint aLength) {
			//int xStart = (RTC.GetMinutes() * 60) + RTC.GetSeconds();
			Hardware.CPU.ZeroFill(aStartAddress, aLength);
			//int xEnd = (RTC.GetMinutes() * 60) + RTC.GetSeconds();
			//int xDiff = xEnd - xStart;
			//Console.Write("Time to clear ");
			//Hardware.Storage.ATAOld.WriteNumber((uint)xDiff, 32);
			//Console.WriteLine("");
		}

		private static void Initialize(uint aStartAddress, uint aEndOfRam) {
			mStart = mStartAddress = aStartAddress + (4 - (aStartAddress % 4));
			mLength = aEndOfRam - aStartAddress;
			mLength = (mLength / 4) * 4;
			mStartAddress += 1024;
			mStartAddress = (mStartAddress / 4) * 4;
			mLength -= 1024;
			//Console.Write("Clearing Memory at ");
			int xCursorLeft = Console.CursorLeft;
			// hack: try to get this working with the full chunk or chunks of 1MB
			//const int xBlockSize = 1024 * 1024;
			//for (uint i = 0; i < (mLength / xBlockSize); i++) {
			//    Console.CursorLeft = xCursorLeft;
			//    Hardware.Storage.ATAOld.WriteNumber(mStartAddress + (i * xBlockSize), 32);
			//    ClearMemory(mStartAddress + (i * xBlockSize), xBlockSize);
			//}
			//Console.Write("Clearing Memory....");
			ClearMemory(aStartAddress, mLength);
			//Console.WriteLine("Done");
			//mFirstBlock = (MemoryBlock*)aStartAddress;
			//mFirstBlock->State = MemoryBlockState.Free;
			//mFirstBlock->Next = (MemoryBlock*)(aStartAddress + mLength);
			//mFirstBlock->Next->State = MemoryBlockState.EndOfMemory;
			DebugUtil.SendMM_Init(mStartAddress, mLength);
		}
		private static bool mInited;
		public static void CheckInit() {
			if (!mInited) {
				mInited = true;
				Initialize(Hardware.CPU.EndOfKernel, (Hardware.CPU.AmountOfMemory * 1024 * 1024) - 1024);
			}
		}

		public static uint MemAlloc(uint aLength) {
			CheckInit();
			uint xTemp = mStartAddress;
			if ((xTemp + aLength) > (mStart + mLength)) {
				Console.WriteLine("Too large memory block allocated!");
				Console.Write("   BlockSize = ");
				Hardware.Storage.ATAOld.WriteNumber(aLength, 32);
				Console.WriteLine("");
				System.Diagnostics.Debugger.Break();
			}
			mStartAddress += aLength;
			DebugUtil.SendMM_Alloc(xTemp, aLength);
			return xTemp;
			//CheckInit();
			//MemoryBlock* xCurrentBlock = mFirstBlock;
			//bool xFound = false;
			//while (!xFound) {
			//    if (xCurrentBlock->State == MemoryBlockState.EndOfMemory) {
			//        DebugUtil.SendError("MM", "Reached maximum memory");
			//        return 0;
			//    }
			//    if (xCurrentBlock->Next == null) {
			//        DebugUtil.SendError("MM", "No next block found, but not yet at EOM", (uint)xCurrentBlock, 32);
			//        return 0;
			//    }
			//    if (((((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock)) >= (aLength + 5)) && (xCurrentBlock->State == MemoryBlockState.Free)) {
			//        xFound = true;
			//        break;
			//    }
			//    xCurrentBlock = xCurrentBlock->Next;
			//}
			//uint xFoundBlockSize = (((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock));
			//if (xFoundBlockSize > (aLength + 37)) {
			//    MemoryBlock* xOldNextBlock = xCurrentBlock->Next;
			//    xCurrentBlock->Next = (MemoryBlock*)(((uint)xCurrentBlock) + aLength + 5);
			//    xCurrentBlock->Next->Next = xOldNextBlock;
			//    xCurrentBlock->Next->State = MemoryBlockState.Free;
			//}
			//xCurrentBlock->State = MemoryBlockState.Used;
			//DebugUtil.SendMM_Alloc((uint)xCurrentBlock, aLength);
			//return ((uint)xCurrentBlock) + 5;
		}

		public static void MemFree(uint aPointer) {
			MemoryBlock* xBlock = (MemoryBlock*)(aPointer - 5);
			DebugUtil.SendMM_Free(aPointer - 5, (((uint)xBlock->Next) - ((uint)xBlock)));
			xBlock->State = MemoryBlockState.Free;
			uint xLength = ((uint)xBlock->Next) - aPointer;
			ClearMemory(aPointer, xLength);
		}
	}
}
