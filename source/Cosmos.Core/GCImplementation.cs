#if DEBUG
//#define GC_DEBUG
#endif
//#define COSMOSDEBUG
using System;
using Cosmos.Core.Memory;

namespace Cosmos.Core
{
    /// <summary>
    /// GCImplementation class. Garbage collector. Mostly not implemented.
    /// </summary>
    /// <remarks>Most of the class is yet to be implemented.</remarks>
    [System.Diagnostics.DebuggerStepThrough]
    public unsafe static class GCImplementation
    {
        private unsafe static byte* memPtr = null;
        private static ulong memLength = 0;
        private static bool StartedMemoryManager = false;
        /// <summary>
        ///
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
        /// Get amount of available Ram
        /// </summary>
        /// <returns>Returns amount of available memory to the System in MB</returns>
        public static ulong GetAvailableRAM()
        {
            return memLength / 1024 / 1024;
        }
        /// <summary>
        /// Get a rough estimate of used Memory by the System
        /// </summary>
        /// <returns>Returns the used PageSize by the MemoryManager in Bytes.</returns>
        public static uint GetUsedRAM()
        {
            return (RAT.TotalPageCount - RAT.GetPageCount((byte)RAT.PageType.Empty)) * RAT.PageSize;
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

            var largestBlock = CPU.GetLargestMemoryBlock();

            if (largestBlock != null)
            {
                memPtr = (byte*)largestBlock->Address;
                memLength = largestBlock->Length;
                if ((uint)memPtr < CPU.GetEndOfKernel() + 1024)
                {
                    memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                    memPtr += RAT.PageSize - (uint)memPtr % RAT.PageSize;
                    memLength = largestBlock->Length - ((uint)memPtr - (uint)largestBlock->Address);
                    memLength += RAT.PageSize - memLength % RAT.PageSize;
                }
            }
            else
            {
                memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                memPtr += RAT.PageSize - (uint)memPtr % RAT.PageSize;
                memLength = 128 * 1024 * 1024;
            }
            RAT.Init(memPtr, (uint)memLength);
        }
        /// <summary>
        /// Get the Pointer of any object needed for Free()
        /// </summary>
        /// <param name="o">Takes any kind of object</param>
        /// <returns>Returns a pointer to the area in memory where the object is located</returns>
        public static unsafe uint* GetPointer(object aObj) => throw null; // this is plugged

        /// <summary>
        /// Get the pointer of any object as a uint
        /// </summary>
        /// <param name="aObj"></param>
        /// <returns></returns>
        public static unsafe uint GetSafePointer(object aObj)
        {
            return (uint)GetPointer(aObj);
        }

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
        /// Increments the root count of the object at the pointer by 1
        /// </summary>
        /// <param name="aPtr"></param>
        public static unsafe void IncRootCount(ushort* aPtr)
        {
            if (RAT.GetPageType(aPtr) != 0)
            {
                var rootCount = *(aPtr - 1) >> 1; // lowest bit is used to set if hit
                *(aPtr - 1) = (ushort)((rootCount + 1) << 1); // loest bit can be zero since we shouldnt be doing this while gc is collecting
            }
        }

        /// <summary>
        /// Decrements the root count of the object at the pointer by 1
        /// </summary>
        /// <param name="aPtr"></param>
        public static unsafe void DecRootCount(ushort* aPtr)
        {
            if (RAT.GetPageType(aPtr) != 0)
            {
                var rootCount = *(aPtr - 1) >> 1; // lowest bit is used to set if hit
                *(aPtr - 1) = (ushort)((rootCount - 1) << 1); // loest bit can be zero since we shouldnt be doing this while gc is collecting
            }
        }

        /// <summary>
        /// Increments the root count of all object stored in this struct by 1
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aType">Type of the struct</param>
        public static unsafe void IncRootCountsInStruct(ushort* aPtr, uint aType)
        {
            uint count = VTablesImpl.GetGCFieldCount(aType);
            uint[] offset = VTablesImpl.GetGCFieldOffsets(aType);
            uint[] types = VTablesImpl.GetGCFieldTypes(aType);
            for (int i = 0; i < count; i++)
            {
                if (VTablesImpl.IsStruct(types[i]))
                {
                    IncRootCountsInStruct(aPtr + offset[i] / 2, types[i]);
                }
                else
                {
                    IncRootCount(*(ushort**)(aPtr + offset[i] / 2));
                }
            }
        }

        /// <summary>
        /// Decrements the root count of all object stored in this struct by 1
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aType">Type of the struct</param>
        public static unsafe void DecRootCountsInStruct(ushort* aPtr, uint aType)
        {
            uint count = VTablesImpl.GetGCFieldCount(aType);
            uint[] offset = VTablesImpl.GetGCFieldOffsets(aType);
            uint[] types = VTablesImpl.GetGCFieldTypes(aType);
            for (int i = 0; i < count; i++)
            {
                if (VTablesImpl.IsStruct(types[i]))
                {
                    DecRootCountsInStruct(aPtr + offset[i] / 2, types[i]);
                }
                else
                {
                    DecRootCount(*(ushort**)(aPtr + offset[i] / 2));
                }
            }
        }
    }
}
