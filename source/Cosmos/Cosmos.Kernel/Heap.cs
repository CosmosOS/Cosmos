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
		private static uint mStartAddress = 4 * 1024 * 1024;
		//		private static uint mCurrentAddress = mStartAddress;
		private static uint mLength = (32 * 1024 * 1024) - mStartAddress;
		private static MemoryBlock* mFirstBlock;
		//private const uint DefaultStartAddress = 4 * 1024 * 1024;
		//private const uint DefaultMaxMemory = 32 * 1024 * 1024;

		private static void ClearMemory(uint aStartAddress, uint aLength) {
			Hardware.CPU.ZeroFill(aStartAddress, aLength);
		}

		private static void Initialize(uint aStartAddress, uint aLength) {
			mStartAddress = aStartAddress + (4 - (aStartAddress % 4));
			mLength = aLength;
			mLength = (mLength / 4) * 4;
			if ((mLength / (1024 * 1024)) > 100) {
				mLength = 64 * 1024 * 1024;
			}
			Console.WriteLine("Initializing Memory");
			ClearMemory(aStartAddress, mLength);
			mFirstBlock = (MemoryBlock*)aStartAddress;
			mFirstBlock->State = MemoryBlockState.Free;
			mFirstBlock->Next = (MemoryBlock*)(aStartAddress + mLength);
			mFirstBlock->Next->State = MemoryBlockState.EndOfMemory;
			DebugUtil.SendMM_Init(mStartAddress, mLength);
		}

		public static void CheckInit() {
			if (mFirstBlock == null) {
				Initialize(Hardware.CPU.EndOfKernel + 4, (Hardware.CPU.AmountOfMemory * 1024 * 1024) - (Hardware.CPU.EndOfKernel + 4));
			}
		}

		[GlueMethod(Type = GlueMethodType.Heap_Alloc)]
		public static uint MemAlloc(uint aLength) {
			CheckInit();
			MemoryBlock* xCurrentBlock = mFirstBlock;
			bool xFound = false;
			while (!xFound) {
				if (xCurrentBlock->State == MemoryBlockState.EndOfMemory) {
					DebugUtil.SendError("MM", "Reached maximum memory");
					return 0;
				}
				if (xCurrentBlock->Next == null) {
					DebugUtil.SendError("MM", "No next block found, but not yet at EOM", (uint)xCurrentBlock, 32);
					return 0;
				}
				if (((((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock)) >= (aLength + 5)) && (xCurrentBlock->State == MemoryBlockState.Free)) {
					xFound = true;
					break;
				}
				xCurrentBlock = xCurrentBlock->Next;
			}
			uint xFoundBlockSize = (((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock));
			if (xFoundBlockSize > (aLength + 37)) {
				MemoryBlock* xOldNextBlock = xCurrentBlock->Next;
				xCurrentBlock->Next = (MemoryBlock*)(((uint)xCurrentBlock) + aLength + 5);
				xCurrentBlock->Next->Next = xOldNextBlock;
				xCurrentBlock->Next->State = MemoryBlockState.Free;
			}
			xCurrentBlock->State = MemoryBlockState.Used;
			DebugUtil.SendMM_Alloc((uint)xCurrentBlock, aLength);
			return ((uint)xCurrentBlock) + 5;
		}

		[GlueMethod(Type = GlueMethodType.Heap_Free)]
		public static void MemFree(uint aPointer) {
			MemoryBlock* xBlock = (MemoryBlock*)(aPointer - 5);
			DebugUtil.SendMM_Free(aPointer - 5, (((uint)xBlock->Next) - ((uint)xBlock)));
			xBlock->State = MemoryBlockState.Free;
			uint xLength = ((uint)xBlock->Next) - aPointer;
			ClearMemory(aPointer, xLength);
		}
	}
}
