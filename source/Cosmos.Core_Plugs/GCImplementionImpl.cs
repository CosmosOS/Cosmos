using System;
using System.Text;
using Cosmos.Core.Memory.Old;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs {
    [Plug(Target = typeof(Cosmos.Core.GCImplementation))]
    public static class GCImplementionImpl {
        public static uint AllocNewObject(uint aSize) {
            GlobalSystemInfo.EnsureInitialized();
            
            return Heap.MemAlloc(aSize);
        }

        public static void IncRefCount(uint aObject) {
            //
        }
        public static void DecRefCount(uint aObject) {
            //
        }

    }
}
