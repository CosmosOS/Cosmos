#if DEBUG
//#define GC_DEBUG
#endif
using System;
using System.Diagnostics;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    [DebuggerStepThrough]
    public static class GCImplementation
    {
        private static void AcquireLock()
        {
            throw new NotImplementedException();
        }

        private static void ReleaseLock()
        {
            throw new NotImplementedException();
        }

        [PlugMethod(PlugRequired = true)]
        public static uint AllocNewObject(uint aSize)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// This function gets the pointer to the memory location of where it's stored.
        /// </summary>
        /// <param name="aObject"></param>
        public static unsafe void IncRefCount(uint aObject)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function gets the pointer to the memory location of where it's stored.
        /// </summary>
        /// <param name="aObject"></param>
        public static unsafe void DecRefCount(uint aObject)
        {
            throw new NotImplementedException();
        }
    }
}
