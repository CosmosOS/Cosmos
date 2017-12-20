using System;
using Cosmos.CPU.x86.Memory.Old;
using IL2CPU.API.Attribs;

namespace Cosmos.CPU_Plugs {
    [Plug(Target = typeof(Cosmos.CPU.x86.GCImplementation))]
    public static class GCImplementationImpl {
        public static uint AllocNewObject(uint aSize) {
            GlobalSystemInfo.EnsureInitialized();
            return Heap.MemAlloc(aSize);
        }

        public static void IncRefCount(uint aObject) {
            // Do nothing right now
        }

        public static void DecRefCount(uint aObject) {
            // Do nothing right now
        }

    }
}
