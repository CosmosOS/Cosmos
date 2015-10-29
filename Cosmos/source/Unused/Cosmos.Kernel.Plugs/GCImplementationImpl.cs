using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs
{
    [Plug(TargetName = "Cosmos.IL2CPU.GCImplementation, Cosmos.IL2CPU")]
    public static class GCImplementationImpl
    {

        public static uint AllocNewObject(uint aSize)
        {
            return GC.AllocNewObject(aSize);
        }

        public static void IncRefCount(uint aObject)
        {
            GC.IncRefCount(aObject);
        }
        public static void DecRefCount(uint aObject)
        {
            GC.DecRefCount(aObject);
        }
	}
}
