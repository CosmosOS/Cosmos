using System;
using System.Text;
using Cosmos.CPU;
using Cosmos.IL2CPU.API;

namespace Cosmos.CPU_Plugs {
    [Plug(Target = typeof(Cosmos.CPU.GCImplementation))]
    public static class GCImplementionImpl {
        public static uint AllocNewObject(uint aSize) {
            //GlobalSystemInfo.EnsureInitialized();
            //return Heap.MemAlloc(aSize);
            return 0;
        }

        public static void IncRefCount(uint aObject) {
        }

        public static void DecRefCount(uint aObject) {
        }

    }
}
