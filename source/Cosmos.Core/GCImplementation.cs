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
        /// Alloc new object. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public static uint AllocNewObject(uint aSize)
        {
            throw new NotImplementedException();

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
    }
}
