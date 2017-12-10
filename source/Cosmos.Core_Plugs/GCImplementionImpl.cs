using System;
using System.Text;
using Cosmos.Core.Memory.Old;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs {
    [Plug(Target = typeof(Cosmos.Core.GCImplementation))]
    public static class GCImplementionImpl {
        public static uint AllocNewObject(uint aSize) {
            //if (Managed_Memory_System.ManagedMemory.SetUpDone == false)
            //{
            //    return Managed_Memory_System.ManagedMemory.SetUpMemoryAlloc(aSize);
            //}
            //else
            //{
            //    return Managed_Memory_System.ManagedMemory.KernelMemAlloc(aSize);
            //}
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
