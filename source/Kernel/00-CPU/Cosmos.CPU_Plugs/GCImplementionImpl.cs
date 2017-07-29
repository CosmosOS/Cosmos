using System;
using System.Text;
using Cosmos.CPU;
using Cosmos.IL2CPU.API;

namespace Cosmos.CPU_Plugs {
    [Plug(Target = typeof(Cosmos.CPU.GCImplementation))]
    public static class GCImplementionImpl {
        public static uint AllocNewObject(uint aSize) {
            Cosmos.CPU.Memory.Old.GlobalSystemInfo.EnsureInitialized();
            return Cosmos.CPU.Memory.Old.Heap.MemAlloc(aSize);
            return 0;
        }

        public static void IncRefCount(uint aObject) {
        }

        public static void DecRefCount(uint aObject) {
        }

    }
}
