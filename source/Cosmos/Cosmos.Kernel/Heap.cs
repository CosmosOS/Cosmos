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
			DebugUtil.SendDoubleNumber("MM", "Clearing memory", aStartAddress, 32, aLength, 32);
			Console.Write("[MM] Clearing ");
			Hardware.Storage.ATA.WriteNumber(aLength, 32);
			Console.Write(" bytes at ");
			Hardware.Storage.ATA.WriteNumber(aStartAddress, 32);
			Console.WriteLine("");
			Hardware.CPU.ZeroFill(aStartAddress, aLength);
			//uint* xPtrLong = (uint*)aStartAddress;
			//{
			//    for (int i = 0; i < (aLength / 4); i++) {
			//        xPtrLong[i] = 0;
			//    }
			//}
			//byte* xPtr = (byte*)(aStartAddress + aLength - (aLength % 4));
			//{
			//    for (int i = 0; i < aLength%4; i++) {
			//        xPtr[i] = 0;
			//    }
			//}

		}

		private static void Initialize(uint aStartAddress, uint aLength) {
			mStartAddress = aStartAddress;
			mLength = aLength;
			ClearMemory(aStartAddress, aLength);
			mFirstBlock = (MemoryBlock*)aStartAddress;
			mFirstBlock->State = MemoryBlockState.Free;
			mFirstBlock->Next = (MemoryBlock*)(aStartAddress + aLength);
			mFirstBlock->Next->State = MemoryBlockState.EndOfMemory;
		}

		public static void CheckInit() {
			if (mFirstBlock == null) {
				Initialize(Hardware.CPU.EndOfKernel + 4, (Hardware.CPU.AmountOfMemory * 1024 * 1024) - (Hardware.CPU.EndOfKernel + 4));
			}
		}

		[GlueMethod(Type = GlueMethodType.Heap_Alloc)]
		public static uint MemAlloc(uint aLength) {
			CheckInit();
			//DebugUtil.SendMM_Alloc(0, aLength);
			MemoryBlock* xCurrentBlock = mFirstBlock;
			bool xFound = false;
			while (!xFound) {
				//DebugUtil.SendNumber("MM", "Checking Block", (uint)xCurrentBlock, 32);
				if (xCurrentBlock->State == MemoryBlockState.EndOfMemory) {
					DebugUtil.SendError("MM", "Reached maximum memory");
					return 0;
				}
				if (xCurrentBlock->Next == null) {
					DebugUtil.SendError("MM", "No next block found, but not yet at EOM");
					return 0;
				}
				if (((((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock)) >= (aLength + 5)) && (xCurrentBlock->State == MemoryBlockState.Free)) {
					xFound = true;
					break;
				}
				xCurrentBlock = xCurrentBlock->Next;
			}
			uint xFoundBlockSize = (((uint)xCurrentBlock->Next) - ((uint)xCurrentBlock));
			//Console.Write("[MM] Found block starts at ");
			//Hardware.Storage.ATA.WriteNumber((uint)xCurrentBlock, 32);
			//Console.WriteLine("");
			// only make a new block when there's significant room left
			if (xFoundBlockSize > (aLength + 37)) {
				MemoryBlock* xOldNextBlock = xCurrentBlock->Next;
				xCurrentBlock->Next = (MemoryBlock*)(((uint)xCurrentBlock) + aLength + 5);
				//Console.Write("[MM] Making new block at ");
				//Hardware.Storage.ATA.WriteNumber((uint)xCurrentBlock->Next, 32);
				//Console.WriteLine("");
				xCurrentBlock->Next->Next = xOldNextBlock;
				xCurrentBlock->Next->State = MemoryBlockState.Free;
			}
			//Console.Write("[MM] Allocating ");
			//Hardware.Storage.ATA.WriteNumber(aLength, 32);
			//Console.Write(" at ");
			//Hardware.Storage.ATA.WriteNumber(((uint)xCurrentBlock), 32);
			//Console.WriteLine("");
			xCurrentBlock->State = MemoryBlockState.Used;
			return ((uint)xCurrentBlock) + 5;
		}

		[GlueMethod(Type = GlueMethodType.Heap_Free)]
		public static void MemFree(uint aPointer) {
			DebugUtil.SendNumber("MM", "Free pointer", aPointer, 32);
			MemoryBlock* xBlock = (MemoryBlock*)(aPointer - 5);
			xBlock->State = MemoryBlockState.Free;
			uint xLength = ((uint)xBlock->Next) - aPointer;
			DebugUtil.SendNumber("MM", "Pointer length", xLength, 32);
			ClearMemory(aPointer, xLength);
		}
	}
}
