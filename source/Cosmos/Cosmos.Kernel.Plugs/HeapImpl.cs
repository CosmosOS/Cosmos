using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(TargetName="Indy.IL2CPU.RuntimeEngine, Indy.IL2CPU")]
	public static class HeapImpl {
		[PlugMethod(Signature="System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___")]
		public static uint Heap_AllocNewObject(uint aSize) {
			return Cosmos.Kernel.Heap.MemAlloc(aSize);
		}

		[PlugMethod(Signature="System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_Free___System_UInt32___")]
		public static void Heap_Free(uint aObject) {
			Kernel.Heap.MemFree(aObject);
		}
	}
}
