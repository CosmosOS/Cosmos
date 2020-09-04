using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// HeapLarge class. Used to alloc and free large memory blocks on the heap.
    /// </summary>
    unsafe static public class HeapLarge
    {
        /// <summary>
        /// Prefix block. Used to store meta information.
        /// </summary>
        public const Native PrefixBytes = 4 * sizeof(Native);

        /// <summary>
        /// Init HeapLarge instance.
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
        static public byte* Alloc(Native aSize)
        {
            Native xPages = (Native)((aSize + PrefixBytes) / RAT.PageSize) + 1;
            var xPtr = (Native*)RAT.AllocPages(RAT.PageType.HeapLarge, xPages);

            xPtr[0] = xPages * RAT.PageSize - PrefixBytes; // Allocated data size
            xPtr[1] = aSize; // Actual data size
            xPtr[2] = 0; // Ref count
            xPtr[3] = 0; // Ptr to first

            return (byte*)xPtr + PrefixBytes;
        }

        /// <summary>
        /// Free block.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <exception cref="Exception">Thrown if page type is not found.</exception>
        static public void Free(void* aPtr)
        {
            // TODO - Should check the page type before freeing to make sure it is a Large?
            // or just trust the caller to avoid adding overhead?
            var xPageIdx = RAT.GetFirstRAT(aPtr);
            RAT.Free(xPageIdx);
        }
    }
}
