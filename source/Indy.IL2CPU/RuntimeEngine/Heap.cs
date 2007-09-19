using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	partial class RuntimeEngine {
		public static IntPtr HeapHandle;
		public const uint InitialHeapSize = 1024;
		public const uint MaximumHeapSize = 10 * InitialHeapSize;
		public static void StartupHeap() {
			HeapHandle = PInvokes.Kernel32_HeapCreate(0, InitialHeapSize, MaximumHeapSize);
		}

		public static void ShutdownHeap() {
			PInvokes.Kernel32_HeapDestroy(HeapHandle);
			HeapHandle = IntPtr.Zero;
		}
	}
}