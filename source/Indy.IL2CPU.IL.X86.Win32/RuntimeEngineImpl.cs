using System;
using System.Linq;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public static class RuntimeEngineImpl {
		public static uint HeapHandle = 0;

		public static void Heap_Initialize() {
			
		}

		public static uint Heap_AllocNewObject(uint aSize) {
			if(HeapHandle == 0) {
				HeapHandle = PInvokes.Kernel32_HeapCreate(0, RuntimeEngine.InitialHeapSize, RuntimeEngine.MaximumHeapSize);
			}
			return PInvokes.Kernel32_HeapAlloc(HeapHandle, 0x00000008, aSize);
		}

		public static void Heap_Free(uint aObject) {
			PInvokes.Kernel32_HeapFree(HeapHandle, 0, aObject);
		}

		public static void Heap_Shutdown() {
			PInvokes.Kernel32_HeapDestroy(HeapHandle);
			HeapHandle = 0;
		}

		public static void ExitProcess(int aExitCode) {
			PInvokes.Kernel32_ExitProcess(aExitCode);
		}
	}
}