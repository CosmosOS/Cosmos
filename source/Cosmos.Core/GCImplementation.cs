#if DEBUG
//#define GC_DEBUG
#endif
//#define COSMOSDEBUG
using System;
using System.Diagnostics;
using Cosmos.Core.Memory;
using Cosmos.IL2CPU;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    /// <summary>
    /// GCImplementation class. Garbage collector. Mostly not implemented.
    /// </summary>
    /// <remarks>Most of the class is yet to be implemented.</remarks>
    [DebuggerStepThrough]
    public unsafe static class GCImplementation
    {
        private unsafe static byte* memPtr = null;
        private static uint memLength = 0;
        private static bool StartedMemoryManager = false;
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
              return (uint)Heap.Alloc(aSize);
            
        }
        /// <summary>
        /// Free Object from Memory
        /// </summary>
        /// <param name="obj">Takes a memory allocated object</param>
        public unsafe static void Free(object aObj)
        {
            Heap.Free(GetPointer(aObj));
        }

        /// <summary>
        /// Increase reference count of an given object
        /// </summary>
        /// <param name="aObject">An object to increase to reference count of.</param>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static unsafe void IncRefCount(uint aObject)
        {
            Heap.IncRefCount((uint*)aObject);
        }

        /// <summary>
        /// Decrease reference count of an given object
        /// </summary>
        /// <param name="aObject">An object to decrease to reference count of.</param>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static unsafe void DecRefCount(uint aObject, uint aId)
        {
            Heap.DecRefCount((uint*)aObject, aId);
        }

        /// <summary>
        /// Decrease reference count of an given object. Even if the reference count reaches 0 the object is not deallocated
        /// </summary>
        /// <param name="aObject">An object to decrease to reference count of.</param>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static unsafe void WeakDecRefCount(uint aObject, uint aId)
        {
            Heap.WeakDecRefCount((uint*)aObject, aId);
        }

        /// <summary>
        /// Decrease reference count of an given object of the given type.
        /// </summary>
        /// <param name="aObject">An object to decrease to reference count of.</param>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static unsafe void DecTypedRefCount(uint aObject, uint aType)
        {
            Heap.DecTypedRefCount((uint*)aObject, aType);
        }

        /// <summary>
        /// Get the number of current references to an object
        /// </summary>
        /// <param name="aObject">Location of the object</param>
        /// <returns>Reference count</returns>
        public static unsafe uint GetRefCount(uint aObject)
        {
            return Heap.GetRefCount((uint*)aObject);
        }

        /// <summary>
        /// Get amount of available Ram
        /// </summary>
        /// <returns>Returns amount of available memory to the System in MB</returns>
        public static uint GetAvailableRAM()
        {
            return memLength / 1024 / 1024;
        }
        /// <summary>
        /// Get a rough estimate of used Memory by the System
        /// </summary>
        /// <returns>Returns the used PageSize by the MemoryManager in Bytes.</returns>
        public static uint GetUsedRAM()
        {
            return (RAT.TotalPageCount - RAT.GetPageCount(RAT.PageType.Empty)) * RAT.PageSize;
        }
        /// <summary>
        /// Initialise the Memory Manager, this should not be called anymore since it is done very early during the boot process.
        /// </summary>
        public static unsafe void Init()
        {
            if (StartedMemoryManager)
            {
                return;
            }
            StartedMemoryManager = true;

            if (CPU.MemoryMapExists())
            {
                var block = CPU.GetLargestMemoryBlock();
                memPtr = (byte*)block->BaseAddr;
                memLength = block->Length;
                if ((uint)memPtr < CPU.GetEndOfKernel() + 1024)
                {
                    memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                    memPtr += RAT.PageSize - (uint)memPtr % RAT.PageSize;
                    memLength = block->Length - ((uint)memPtr - (uint)block->BaseAddr);
                    memLength += RAT.PageSize - memLength % RAT.PageSize;
                }
            }
            else
            {
                memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                memPtr += RAT.PageSize - (uint)memPtr % RAT.PageSize;
                memLength = (128 * 1024 * 1024);
            }
            RAT.Init(memPtr, memLength);
            
        }
        /// <summary>
        /// Get the Pointer of any object needed for Free()
        /// </summary>
        /// <param name="o">Takes any kind of object</param>
        /// <returns>Returns a pointer to the area in memory where the object is located</returns>
        public static unsafe uint* GetPointer(object aObj) => throw null; // this is plugged

        /// <summary>
        /// Get cosmos internal type from object
        /// </summary>
        /// <param name="aObj"></param>
        /// <returns></returns>
        public static unsafe uint GetType(object aObj)
        {
            return *GetPointer(aObj);
        }

        /// <summary>
        /// Clean up the remains of an object/value at a location
        /// </summary>
        /// <param name="aPtr">Pointer to the first byte of the location</param>
        /// <param name="aType">Type of the object being cleaned up</param>
        public static unsafe void CleanupTypedObject(void* aPtr, uint aType)
        {
            HeapSmall.CleanupTypedObject(aPtr, aType);
        }

        /// <summary>
        /// Increment the GC References of objects stored in a struct
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aType"></param>
        [NoGC()]
        public static unsafe void IncStructFieldReferences(void* aPtr, uint aType)
        {
            uint fields = VTablesImpl.GetGCFieldCount(aType);
            var offsets = VTablesImpl.GetGCFieldOffsets(aType);
            var types = VTablesImpl.GetGCFieldTypes(aType);
            var obj = (uint*)aPtr;
            for (int i = 0; i < fields; i++)
            {
                if (VTablesImpl.IsValueType(types[i]))
                {
                    var location = obj + offsets[i] / 4 + 1; // +1 since we are only using 32bits from the 64bit
                    if (*location != 0) // Check if its null
                    {
                        location = *(uint**)location;
                        if (RAT.GetPageType(location) == RAT.PageType.HeapSmall)
                        {
                            Heap.DecRefCount(location, 0);
                        }
                    }
                }
                else
                {
                    IncStructFieldReferences(obj + offsets[i] / 4, types[i]);
                }
            }
        }

    }
}
