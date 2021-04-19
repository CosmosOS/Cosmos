#if DEBUG
//#define GC_DEBUG
#endif
using System;
using System.Diagnostics;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    /// <summary>
    /// GCImplementation class. Garbage collector. Mostly not implemented.
    /// </summary>
    /// <remarks>Most of the class is yet to be implemented.</remarks>
    [DebuggerStepThrough]
    public static class GCImplementation
    {
        private static bool isInitialized;
        /// <summary>
        /// Acquire lock. Not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        private static void AcquireLock()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Release lock. Not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        private static void ReleaseLock()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Alloc new object.
        /// </summary>
        public unsafe static uint AllocNewObject(uint aSize)
        {


                if (!isInitialized)
                {
                isInitialized = true;
                Init();
                return (uint)Memory.Heap.Alloc(aSize);
            }
           
            else
            {
                return (uint)Memory.Heap.Alloc(aSize);
            }

        }

        /// <summary>
        /// Increase reference count of an given object. Plugged.
        /// </summary>
        /// <param name="aObject">An object to increase to reference count of.</param>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static unsafe void IncRefCount(uint aObject)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrease reference count of an given object. Plugged.
        /// </summary>
        /// <param name="aObject">An object to decrease to reference count of.</param>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static unsafe void DecRefCount(uint aObject)
        {
            throw new NotImplementedException();
        }

        public static unsafe void Init()
        {

           
            byte* memPtr = (byte*)CPU.GetEndOfKernel();
            memPtr += Memory.RAT.PageSize - (uint)memPtr % Memory.RAT.PageSize;
            Memory.RAT.Init(memPtr,(512 * 1024 * 1024));
            
        }

    }
}
