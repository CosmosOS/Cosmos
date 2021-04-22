#if DEBUG
#define GC_DEBUG
#endif
#define COSMOSDEBUG
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
    public unsafe static class GCImplementation
    {
        private static bool isInitialized;
        private unsafe static byte* memPtr = null;
        private static uint memLength = 0;
        private static bool AllocAfterInit;
        private static uint saveSize;
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
        public static uint FallBackInit(uint aSize)
        {
            return (uint)Memory.Heap.Alloc(aSize);
        }

        /// <summary>
        /// Alloc new object.
        /// </summary>
        public unsafe static uint AllocNewObject(uint aSize)
        {
            ///This is a hack, we should really find out why Init is trying to allocate,
            ///this shouldnt be but for now this works
        /*    if (!isInitialized)
            {
                AllocAfterInit = true;
                saveSize = aSize;
                Init();
               
            }*/
              return (uint)Memory.Heap.Alloc(aSize);
            
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

        public static uint GetDetectedRam()
        {
                uint memLength2;
                var block = CPU.GetLargestMemoryBlock();
               memLength2 = block->Length  - ((uint)memPtr - (uint)block->BaseAddr);
               memLength2 += Memory.RAT.PageSize - memLength % Memory.RAT.PageSize;
            return memLength2 / 1024 / 1024;
        }
        public static uint GetAmountOfRam()
        {
            return memLength / 1024 / 1024;
        }

        public static unsafe void Init()
        {
            if(CPU.MemoryMapExists())
            {
                var block = CPU.GetLargestMemoryBlock();
                memPtr = (byte*)block->BaseAddr;
                memLength = block->Length;
                Debug.Kernel.Debugger.DoSendNumber((uint)memPtr);
                Debug.Kernel.Debugger.DoSendNumber(memLength);
                if ((uint)memPtr < (uint)CPU.GetEndOfKernel() + 1024)
                {
                    memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                    memPtr += Memory.RAT.PageSize - (uint)memPtr % Memory.RAT.PageSize;
                    memLength = block->Length - ((uint)memPtr - (uint)block->BaseAddr);
                    memLength += Memory.RAT.PageSize - memLength % Memory.RAT.PageSize;
                    Debug.Kernel.Debugger.DoSendNumber((uint)memPtr);
                    Debug.Kernel.Debugger.DoSendNumber(memLength);
                }
            }
            else
            {
                memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                memPtr += Memory.RAT.PageSize - (uint)memPtr % Memory.RAT.PageSize;
                memLength = (128 * 1024 * 1024);
                Debug.Kernel.Debugger.DoSendNumber((uint)memPtr);
                Debug.Kernel.Debugger.DoSendNumber(memLength);
            }
            Debug.Kernel.Debugger.DoSendNumber((uint)memPtr);
            Debug.Kernel.Debugger.DoSendNumber(memLength);
            Memory.RAT.Init(memPtr,memLength);
         /*   if(AllocAfterInit)
            {
                isInitialized = true;
                FallBackInit(saveSize);
                AllocAfterInit = false;
            }
            isInitialized = true;*/
            
        }

    }
}
