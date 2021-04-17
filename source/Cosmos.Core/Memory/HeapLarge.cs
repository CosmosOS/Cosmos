using System;
using System.Linq;
using System.Threading.Tasks;
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
            uint xPages = ((aSize + PrefixBytes) / RAT.PageSize) + 1;
            var xPtr = (uint*)RAT.AllocPages(aType, xPages);

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
    }
}
