//#define COSMOSDEBUG
using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(Buffer))]
    public class BufferImpl
    {
        static Debugger mDebugger = new Debugger("Plug", "Buffer");
        /// <summary>
        /// The memmove() function copies n bytes from memory area src to memory area dest.
        /// The memory areas may overlap: copying takes place as though the bytes in src
        /// are first copied into a temporary array that does not overlap src or dest,
        /// and the bytes are then copied from the temporary array to dest.
        /// </summary>
        /// <param name="aDest">Destination address to copy data into.</param>
        /// <param name="aSrc">Source address from where copy data.</param>
        /// <param name="aCount">Count of bytes to copy.</param>
        [PlugMethod(IsOptional = true, Signature = "System_Void__System_Buffer___Memmove_System_Byte___System_Byte___System_UIntPtr_")]
        public static unsafe void __Memmove(byte* aDest, byte* aSrc, uint aCount)
        {
            MemoryOperations.MemoryOperationsImpl.Copy(aDest, aSrc, (int)aCount);
        }

        /// <summary>
        /// The memmove() function copies n bytes from memory area src to memory area dest.
        /// The memory areas may overlap: copying takes place as though the bytes in src
        /// are first copied into a temporary array that does not overlap src or dest,
        /// and the bytes are then copied from the temporary array to dest.
        /// </summary>
        /// <param name="dest">Destination address to copy data into.</param>
        /// <param name="src">Source address from where copy data.</param>
        /// <param name="count">Count of bytes to copy.</param>
        [PlugMethod(IsOptional = true)]
        public static unsafe void __Memmove(byte* dest, byte* src, ulong count)
        {
            // TODO: Cast could cause a loss of data.
            __Memmove(dest, src, (uint)count);
        }

        [PlugMethod(Signature = "System_Void__System_Buffer___BulkMoveWithWriteBarrier__System_Byte___System_Byte__System_UIntPtr_")]
        public static unsafe void __BulkMoveWithWriteBarrier(ref byte destination, ref byte source, uint byteCount)
        {
            fixed (byte* srcPtr = &source)
            fixed (byte* dstPtr = &destination)
            {
                for (int i = 0; i < byteCount; i++)
                {
                    dstPtr[i] = srcPtr[i];
                }
            }

            // Unsafe.CopyBlock(ref destination, ref source, byteCount);
        }

        public static unsafe void __ZeroMemory(void* aPtr, UIntPtr aLength)
        {
            CPU.ZeroFill((uint)aPtr, *(uint*)aLength.ToPointer()); 
        }
    }
}
