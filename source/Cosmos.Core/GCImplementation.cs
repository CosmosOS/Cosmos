#if DEBUG
#define GC_DEBUG
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
    public unsafe static class GCImplementation
    {
        private static bool isInitialized;
        private unsafe static byte* memPtr = null;
        private static uint memLength = 0;
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
            // Debug.Kernel.Debugger.DoBochsBreak();
            //Debug.Kernel.Debugger.DoSendNumber((uint)CPU.GetLargestMemoryBlock());
            //Debug.Kernel.Debugger.DoSendNumber((uint)CPU.GetLargestMemoryBlock()->Length * 1024);
            //Debug.Kernel.Debugger.DoSendNumber((uint)CPU.GetLargestMemoryBlock()->BaseAddr);
            //Debug.Kernel.Debugger.DoSendNumber((uint)Bootstrap.MultibootHeader->memMapAddress);
            //Debug.Kernel.Debugger.DoBochsBreak();
            /* if (!isInitialized)
             {
                 isInitialized = true;
                 Init();
             }*/
            if (isInitialized)
            {
                return (uint)Memory.Heap.Alloc(aSize);
            }
            else
            {
                //We really shouldnt be here, something bad happened uh oh
                Cosmos.Debug.Kernel.Debugger.SendKernelPanic(0x99999);
                return 0;
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
            if(CPU.MemoryMapExists())
            {
                var block = CPU.GetLargestMemoryBlock();
                Debug.Kernel.Debugger.DoSendNumber((uint)block);
                memPtr = (byte*)block->BaseAddr;
                memLength = block->Length * 1024;
                if((uint)memPtr < (uint)CPU.GetEndOfKernel() + 1024)
                {
                    memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                    memPtr += Memory.RAT.PageSize - (uint)memPtr % Memory.RAT.PageSize;
                    memLength = block->Length * 1024 - ((uint)memPtr - (uint)block->BaseAddr);
                    memLength += Memory.RAT.PageSize - memLength % Memory.RAT.PageSize;
                }
            }
            else
            {
                memPtr = (byte*)CPU.GetEndOfKernel() + 1024;
                memPtr += Memory.RAT.PageSize - (uint)memPtr % Memory.RAT.PageSize;
                memLength = (128 * 1024 * 1024);
            }
            Debug.Kernel.Debugger.DoSendNumber((uint)memPtr);
            Debug.Kernel.Debugger.DoSendNumber(memLength);
            Memory.RAT.Init(memPtr,memLength);
            isInitialized = true;
            
        }

    }
}
