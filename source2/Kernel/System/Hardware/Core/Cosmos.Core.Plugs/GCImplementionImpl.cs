using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs
{
    [Plug(TargetName = "Cosmos.IL2CPU.GCImplementation, Cosmos.IL2CPU")]
    public static class GCImplementionImpl
    {
        public static uint AllocNewObject(uint aSize)
        {
            //if (Managed_Memory_System.ManagedMemory.SetUpDone == false)
            //{
            //    return Managed_Memory_System.ManagedMemory.SetUpMemoryAlloc(aSize);
            //}
            //else
            //{
            //    return Managed_Memory_System.ManagedMemory.KernelMemAlloc(aSize);
            //}
            return Heap.MemAlloc(aSize);
        }

        public static void IncRefCount(uint aObject)
        {
            //
        }
        public static void DecRefCount(uint aObject)
        {
            //
        }

    }
}
