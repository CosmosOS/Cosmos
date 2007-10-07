using System;
using System.Linq;

namespace Indy.IL2CPU.IL.Win32 {
	public static class RuntimeEngineImpl {
		public static uint HeapHandle = 0;

		public static void Heap_Initialize() {
			HeapHandle = PInvokes.Kernel32_HeapCreate(0, RuntimeEngine.InitialHeapSize, RuntimeEngine.MaximumHeapSize);
		}

		public static uint Heap_AllocNewObject(uint aSize) {
			return PInvokes.Kernel32_HeapAlloc(HeapHandle, 0x00000008, aSize);
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