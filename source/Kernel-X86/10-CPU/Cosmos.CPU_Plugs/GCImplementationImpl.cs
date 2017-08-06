using System;
using Cosmos.CPU;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.CPU_Plugs {
    [Plug(Target = typeof(GCImplementation))]
    public static class GCImplementationImpl {
        public static uint AllocNewObject(uint aSize) {
            CPU.Memory.Old.GlobalSystemInfo.EnsureInitialized();
            return CPU.Memory.Old.Heap.MemAlloc(aSize);
        }

        public static void IncRefCount(uint aObject) {
            // Do nothing right now
        }

        public static void DecRefCount(uint aObject) {
            // Do nothing right now
        }

    }
}
