using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(TargetName = "Cosmos.IL2CPU.RuntimeEngine, Cosmos.IL2CPU")]
	public static class HeapImpl {
		//[PlugMethod(Signature = "System_UInt32__Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject_System_UInt32_")]
		public static uint Heap_AllocNewObject(uint aSize) {
			return Heap.MemAlloc(aSize);
		}

		//[PlugMethod(Signature = "System_UInt32__Indy_IL2CPU_RuntimeEngine_Heap_Free_System_UInt32_")]
		public static void Heap_Free(uint aObject) {
			//Heap.MemFree(aObject);
		}
	}
}
