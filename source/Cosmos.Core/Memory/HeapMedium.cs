using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// HeapMedium class. Used to alloc and free medium memory blocks on the heap.
    /// </summary>
    /// For now the HeapMedium does exactly the same as HeapLarge
    unsafe static public class HeapMedium
    {
        /// <summary>
        /// Number of prefix bytes for the heap.
        /// </summary>
        public const uint PrefixBytes = 4 * sizeof(uint);
        // TODO Adjust when page size changes from 4k to 2/4mb
        // HeapMedium may be of limited use with 4k pages depending on the final sizes of the small heap.
        /// <summary>
        /// Max item size in a page.
        /// </summary>
        public const uint MaxItemSize = RAT.PageSize - PrefixBytes;

        /// <summary>
        /// Init HeapMedium instance.
        /// </summary>
        /// <remarks>Empty function</remarks>
        static public void Init()
        {
        }

        /// <summary>
        /// Alloc memory block, of a given size.
        /// </summary>
        /// <param name="aSize">A size of block to alloc, in bytes.</param>
        /// <returns>Byte pointer to the start of the block.</returns>
        static public byte* Alloc(uint aSize)
        {
            return HeapLarge.Alloc(aSize, RAT.PageType.HeapMedium);
        }

        /// <summary>
        /// Free block.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <exception cref="Exception">Thrown if page type is not found.</exception>
        static public void Free(void* aPtr)
        {
            HeapLarge.Free(aPtr);
        }
    }
}
