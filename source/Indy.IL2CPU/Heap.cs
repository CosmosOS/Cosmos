using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	partial class RuntimeEngine {
		public static uint HeapHandle = 0;
		public const uint InitialHeapSize = 4096;
		public const uint MaximumHeapSize = 10 * 1024 * InitialHeapSize; // 10 megabytes
		public static void Heap_Initialize() {
			//HeapHandle = PInvokes.Kernel32_HeapCreate(0, InitialHeapSize, MaximumHeapSize);
		}

		public static uint Heap_AllocNewObject(uint aSize) {
//			if (aSize == 0) {
//				aSize = 1;
//			}
			//return PInvokes.Kernel32_HeapAlloc(HeapHandle, 0x00000008, aSize);
			return 0;
		}

		public static void Heap_Free(uint aObject) {
			//
		}

		public static void Heap_Shutdown() {
			//PInvokes.Kernel32_HeapDestroy(HeapHandle);
			//HeapHandle = 0;
		}
	}
}