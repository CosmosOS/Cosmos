using System;
using Cosmos.Core;

namespace Cosmos.Kernel
{
    public class GC
    {
        public static uint AllocNewObject(uint aSize)
        {
            return Heap.MemAlloc(aSize);
        }
        public static unsafe void IncRefCount(uint aObject)
        {
            //throw new NotImplementedException();
        }
        public static unsafe void DecRefCount(uint aObject)
        {
            //throw new NotImplementedException();
        }
    }
}
