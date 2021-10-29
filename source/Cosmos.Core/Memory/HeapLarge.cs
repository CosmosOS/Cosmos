using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;
using Native = System.UInt32;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// HeapLarge class. Used to alloc and free large memory blocks on the heap.
    /// </summary>
    public static unsafe class HeapLarge
    {
        /// <summary>
        /// Prefix block. Used to store meta information.
        /// </summary>
        public const uint PrefixBytes = 4 * sizeof(uint);

        /// <summary>
        /// Init HeapLarge instance.
        /// </summary>
        /// <remarks>Empty function</remarks>
        public static void Init()
        {
        }

        /// <summary>
        /// Alloc memory block, of a given size.
        /// </summary>
        /// <param name="aSize">A size of block to alloc, in bytes.</param>
        /// <returns>Byte pointer to the start of the block.</returns>
        public static byte* Alloc(uint aSize, byte aType = RAT.PageType.HeapLarge)
        {
            Debugger.DoSendNumber(0xB16A220C);
            Debugger.DoSendNumber(aSize);
            uint xPages = ((aSize + PrefixBytes) / RAT.PageSize) + 1;
            Debugger.DoSendNumber(xPages);
            var xPtr = (uint*)RAT.AllocPages(aType, xPages);
            if(xPtr == null)
            {
                Debugger.SendKernelPanic(0x67); // out of pages
            }
            xPtr[0] = xPages * RAT.PageSize - PrefixBytes; // Allocated data size
            xPtr[1] = aSize; // Actual data size
            xPtr[2] = 1; // Ref count
            xPtr[3] = 0; // padding for now?,
            return (byte*)xPtr + PrefixBytes;
        }

        /// <summary>
        /// Free block.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <exception cref="Exception">Thrown if page type is not found.</exception>
        public static void Free(void* aPtr)
        {
            var xPageIdx = RAT.GetFirstRAT(aPtr);
            RAT.Free(xPageIdx);
        }

        /// <summary>
        /// Increment the reference count for an object stored on the large heap
        /// </summary>
        /// <param name="aPtr">Pointer to the object</param>
        public static void IncRefCount(void* aPtr)
        {
            uint* obj = (uint*)aPtr;
            obj[-2]++;
        }


        /// <summary>
        /// Get the reference count for an object stored on the large heap
        /// </summary>
        /// <param name="aPtr">Pointer to the object</param>
        public static uint GetRefCount(void* aPtr)
        {
            uint* obj = (uint*)aPtr;
            return obj[-2];
        }

        /// <summary>
        /// Decrement the reference count for an object stored on the large heap
        /// Frees the object if ref count reaches 0
        /// </summary>
        /// <param name="aPtr">Pointer to the object</param>
        public static void DecRefCount(void* aPtr)
        {
            uint* obj = (uint*)aPtr;
            obj[-2]--;
            if (obj[-2] == 0)
            {
                Debugger.DoSendNumber(0x22);
                Debugger.DoSendNumber((uint)obj);
                Free(aPtr);
            }
        }

        /// <summary>
        /// Decrement the reference count for an object stored on the large heap
        /// DDOES not free the object if ref count reaches 0
        /// </summary>
        /// <param name="aPtr">Pointer to the object</param>
        public static void WeakDecRefCount(void* aPtr)
        {
            uint* obj = (uint*)aPtr;
            obj[-2]--;
        }

        /// <summary>
        /// Decrement the reference count for an object of the given type
        /// Not Implemented!
        /// </summary>
        /// <param name="aPtr"></param>
        public static void DecTypedRefCount(void* aPtr, uint aType)
        {
            throw new NotImplementedException();
        }
    }
}
