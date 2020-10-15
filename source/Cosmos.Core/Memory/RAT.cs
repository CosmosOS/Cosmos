using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// RAT (RAM Allocation Table) class.
    /// </summary>
    unsafe static public class RAT
    {
        // RAT: RAM Allocation Table
        //
        // A byte table which defines the code which owns the page.
        // Owners can further subdivide table types on their own and RAT
        // code must not assume anything about contents of pages other
        // than who owns them.

        /// <summary>
        /// PageType class. Used like a enum to define the type of the page.
        /// </summary>
        /// <remarks>Only used to define page type.</remarks>
        static public class PageType
        {
            /// <summary>
            /// Empty page.
            /// </summary>
            public const byte Empty = 0;

            // Data Types from 1, special meanings from 255 down.
            /// <summary>
            /// RAT type page.
            /// </summary>
            public const byte RAT = 1;
            /// <summary>
            /// Small heap page.
            /// </summary>
            public const byte HeapSmall = 2;
            /// <summary>
            /// Medium heap page.
            /// </summary>
            public const byte HeapMedium = 3;
            /// <summary>
            /// Large heap page.
            /// </summary>
            public const byte HeapLarge = 4;
            // Code
            // Stack
            // Disk Cache

            // Extension of previous page.
            /// <summary>
            /// Extension of pre-existing page.
            /// </summary>
            public const byte Extension = 255;
        }

        /// <summary>
        /// Debug flag.
        /// </summary>
        /// <remarks>Used to bypass certain checks that will fail during tests and debugging.</remarks>
        static internal bool Debug = false;

        /// <summary>
        /// Native Intel page size.
        /// </summary>
        /// <remarks><list type="bullet">
        /// <item>x86 Page Size: 4k, 2m (PAE only), 4m.</item>
        /// <item>x64 Page Size: 4k, 2m</item>
        /// </list></remarks>
        public const Native PageSize = 4096;

        /// <summary>
        /// Start of area usable for heap, and also start of heap.
        /// </summary>
        static private byte* mRamStart;
        /// <summary>
        /// Size of heap.
        /// </summary>
        static private Native mRamSize;
        /// <summary>
        /// Number of pages in the heap.
        /// </summary>
        /// <remarks>Calculated from mSize.</remarks>
        static private Native mPageCount;

        /// <summary>
        /// Pointer to the RAT.
        /// </summary>
        /// <remarks>Covers Data area only.</remarks>
        // We need a pointer as the RAT can move around in future with dynamic RAM etc.
        static private byte* mRAT;

        /// <summary>
        /// Init RAT.
        /// </summary>
        /// <param name="aStartPtr">A pointer to the start of the heap.</param>
        /// <param name="aSize">A heap size, in bytes.</param>
        /// <exception cref="Exception">Thrown if:
        /// <list type="bullet">
        /// <item>RAM start or size is not page aligned.</item>
        /// </list>
        /// </exception>
        static public void Init(byte* aStartPtr, Native aSize)
        {
            if ((Native)aStartPtr % PageSize != 0 && !Debug)
            {
                throw new Exception("RAM start must be page aligned.");
            }

            if (aSize % PageSize != 0)
            {
                throw new Exception("RAM size must be page aligned.");
            }

            mRamStart = aStartPtr;
            mRamSize = aSize;
            mPageCount = aSize / PageSize;

            // We need one status byte for each block.
            // Intel blocks are 4k (10 bits). So for 4GB, this means
            // 32 - 12 = 20 bits, 1 MB for a RAT for 4GB. 0.025%
            Native xRatPageCount = (mPageCount - 1) / PageSize + 1;
            Native xRatPageBytes = xRatPageCount * PageSize;
            mRAT = mRamStart + mRamSize - xRatPageBytes;
            for (byte* p = mRAT; p < mRAT + xRatPageBytes - xRatPageCount; p++)
            {
                *p = PageType.Empty;
            }
            for (byte* p = mRAT + xRatPageBytes - xRatPageCount; p < mRAT + xRatPageBytes; p++)
            {
                *p = PageType.RAT;
            }

            Heap.Init();
        }

        /// <summary>
        /// Get page count.
        /// </summary>
        /// <param name="aType">A page type to count.</param>
        /// <returns>Native value.</returns>
        static public Native GetPageCount(byte aType = 0)
        {
            Native xResult = 0;
            byte xType = 0; // Could us nullable type instead of this + xCounting, but this is faster.
            bool xCounting = false;
            for (byte* p = mRAT; p < mRAT + mPageCount; p++)
            {
                if (*p == aType)
                {
                    xType = *p;
                    xResult++;
                    xCounting = true;
                }
                else if (xCounting)
                {
                    if (xType == PageType.Extension)
                    {
                        xResult++;
                    }
                    else
                    {
                        xCounting = false;
                    }
                }
            }
            return xResult;
        }

        /// <summary>
        /// Alloc a block with a given type and size.
        /// </summary>
        /// <param name="aType">A type of block to alloc.</param>
        /// <param name="aBytes">Number of bytes to alloc.</param>
        /// <returns>A pointer to the first page on success, null on failure.</returns>
        static public void* AllocBytes(byte aType, Native aBytes)
        {
            return AllocPages(aType, aBytes / PageSize);
        }

        /// <summary>
        /// Alloc a given number of pages, all of the same type.
        /// </summary>
        /// <param name="aType">A type of pages to alloc.</param>
        /// <param name="aPageCount">Number of pages to alloc. (default = 1)</param>
        /// <returns>A pointer to the first page on success, null on failure.</returns>
        static public void* AllocPages(byte aType, Native aPageCount = 1)
        {
            byte* xPos = null;

            // Could combine with an external method or delegate, but will slow things down
            // unless we can force it to be inlined.
            //
            // Alloc single blocks at bottom, larger blocks at top to help reduce fragmentation.
            Native xCount = 0;
            if (aPageCount == 1)
            {
                for (byte* p = mRAT; p < mRAT + mPageCount; p++)
                {
                    if (*p == PageType.Empty)
                    {
                        xCount++;
                        if (xCount == aPageCount)
                        {
                            xPos = p - xCount + 1;
                            break;
                        }
                    }
                    else
                    {
                        xCount = 0;
                    }
                }
            }
            else
            {
                // This loop will FAIL if mRAT is ever 0. This should be impossible though
                // so we don't bother to account for such a case. xPos would also have issues.
                for (byte* p = mRAT + mPageCount - 1; p >= mRAT; p--)
                {
                    if (*p == PageType.Empty)
                    {
                        xCount++;
                        if (xCount == aPageCount)
                        {
                            xPos = p;
                            break;
                        }
                    }
                    else
                    {
                        xCount = 0;
                    }
                }
            }

            // If we found enough space, mark it as used.
            if (xPos != null)
            {
                byte* xResult = mRamStart + (xPos - mRAT) * PageSize;
                *xPos = aType;
                for (byte* p = xPos + 1; p < xPos + xCount; p++)
                {
                    *p = PageType.Extension;
                }
                return xResult;
            }

            return null;
        }

        /// <summary>
        /// Get the first RAT address.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <returns>Native value.</returns>
        /// <exception cref="Exception">Thrown if page type is not found.</exception>
        static public Native GetFirstRAT(void* aPtr)
        {
            var xPos = (Native)((byte*)aPtr - mRamStart) / RAT.PageSize;
            // See note about when mRAT = 0 in Alloc.
            for (byte* p = mRAT + xPos; p >= mRAT; p--)
            {
                if (*p != PageType.Extension)
                {
                    return (Native)(p - mRAT);
                }
            }
            throw new Exception("Page type not found. Likely RAT is rotten.");
        }

        /// <summary>
        /// Get the page type pointed by a pointer.
        /// </summary>
        /// <param name="aPtr">A pointer to the page to get the type of.</param>
        /// <returns>byte value.</returns>
        /// <exception cref="Exception">Thrown if page type is not found.</exception>
        static public byte GetPageType(void* aPtr)
        {
            return mRAT[GetFirstRAT(aPtr)];
        }

        /// <summary>
        /// Free page.
        /// </summary>
        /// <param name="aPageIdx">A index to the page to be freed.</param>
        static public void Free(Native aPageIdx)
        {
            byte* p = mRAT + aPageIdx;
            *p = PageType.Empty;
            for (; p < mRAT + mPageCount; p++)
            {
                if (*p != PageType.Extension)
                {
                    break;
                }
                *p = PageType.Empty;
            }
        }
    }
}
