using System;
using Cosmos.Debug.Kernel;
using Native = System.UInt32;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// RAT (RAM Allocation Table) class for managing memory allocation and tracking page types.
    /// </summary>
    unsafe static public class RAT
    {
        // RAT: RAM Allocation Table
        //
        // A byte table that defines the code which owns each page.
        // Owners can further subdivide table types on their own, and RAT
        // code must not assume anything about the contents of pages other
        // than who owns them.

        /// <summary>
        /// PageType enum. Used to define the type of a page.
        /// </summary>
        public enum PageType
        {
            /// <summary>
            /// Empty page.
            /// Can also indicate an invalid page.
            /// </summary>
            Empty = 0,

            // Data Types from 1, special meanings from 255 down.
            /// <summary>
            /// Indicates that the page contains objects managed by the GC.
            /// </summary>
            GCManaged = 1,
            /// <summary>
            /// Small heap page.
            /// </summary>
            HeapSmall = 3,
            /// <summary>
            /// Medium heap page.
            /// </summary>
            HeapMedium = 5,
            /// <summary>
            /// Large heap page.
            /// </summary>
            HeapLarge = 7,

            /// <summary>
            /// RAT type page.
            /// </summary>
            RAT = 32,
            /// <summary>
            /// Page which is part of the SMT.
            /// </summary>
            SMT = 64,
            // Extension of previous page.
            /// <summary>
            /// Extension of a pre-existing page.
            /// </summary>
            Extension = 128,
        }

        /// <summary>
        /// Debug flag.
        /// </summary>
        /// <remarks>Used to bypass certain checks that may fail during tests and debugging.</remarks>
        static internal bool Debug = false;

        /// <summary>
        /// Native Intel page size.
        /// </summary>
        /// <remarks>
        /// x86 Page Sizes: 4k, 2m (PAE only), 4m.
        /// x64 Page Sizes: 4k, 2m.
        /// </remarks>
        public const uint PageSize = 4096;

        /// <summary>
        /// Start of the area usable for heap, and also the start of the heap.
        /// </summary>
        public static byte* RamStart;
        /// <summary>
        /// Pointer to the end of the heap.
        /// </summary>
        public static byte* HeapEnd;
        /// <summary>
        /// Size of the heap.
        /// </summary>
        public static uint RamSize;
        /// <summary>
        /// Number of pages in the heap.
        /// </summary>
        /// <remarks>Calculated from RamSize.</remarks>
        static public uint TotalPageCount;

        /// <summary>
        /// Pointer to the RAT.
        /// </summary>
        /// <remarks>Covers the data area only.</remarks>
        // We need a pointer as the RAT can move around in the future with dynamic RAM, etc.
        public static byte* mRAT;

        /// <summary>
        /// Initializes the RAT with the specified heap start address and size.
        /// </summary>
        /// <param name="aStartPtr">A pointer to the start of the heap.</param>
        /// <param name="aSize">The heap size, in bytes.</param>
        /// <exception cref="Exception">Thrown if:
        /// <list type="bullet">
        /// <item>RAM start or size is not page-aligned.</item>
        /// </list>
        /// </exception>
        public static void Init(byte* aStartPtr, uint aSize)
        {
            if ((uint)aStartPtr % PageSize != 0 && !Debug)
            {
                Debugger.DoSendNumber((uint)aStartPtr % PageSize);
                Debugger.DoBochsBreak();
                throw new Exception("RAM start must be page-aligned.");
            }

            if (aSize % PageSize != 0)
            {
                Debugger.DoSendNumber(aSize % PageSize);
                Debugger.SendKernelPanic(11);
                throw new Exception("RAM size must be page-aligned.");
            }

            RamStart = aStartPtr;
            RamSize = aSize;
            HeapEnd = aStartPtr + aSize;
            TotalPageCount = aSize / PageSize;

            // We need one status byte for each block.
            // Intel blocks are 4k (10 bits). So for 4GB, this means
            // 32 - 12 = 20 bits, 1 MB for a RAT for 4GB. 0.025%
            uint xRatPageCount = (TotalPageCount - 1) / PageSize + 1;
            uint xRatTotalSize = xRatPageCount * PageSize;
            mRAT = RamStart + RamSize - xRatTotalSize;

            // Mark empty pages as such in the RAT Table
            for (byte* p = mRAT; p < mRAT + TotalPageCount - xRatPageCount; p++)
            {
                *p = (byte)PageType.Empty;
            }
            // Mark the RAT pages as such
            for (byte* p = mRAT + TotalPageCount - xRatPageCount; p < mRAT + xRatTotalSize; p++)
            {
                *p = (byte)PageType.RAT;
            }
            Heap.Init();
        }

        /// <summary>
        /// Gets the count of pages with the specified page type.
        /// </summary>
        /// <param name="aType">The page type to count.</param>
        /// <returns>The number of pages of this type, including extension pages.</returns>
        public static uint GetPageCount(byte aType = 0)
        {
            uint xResult = 0;
            bool xCounting = false;
            for (byte* p = mRAT; p < mRAT + TotalPageCount; p++)
            {
                if (*p == aType)
                {
                    xResult++;
                    xCounting = true;
                }
                else if (xCounting)
                {
                    if (*p == (byte)PageType.Extension)
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
        /// Allocates a given number of pages, all of the same type.
        /// </summary>
        /// <param name="aType">The type of pages to allocate.</param>
        /// <param name="aPageCount">The number of pages to allocate. (default = 1)</param>
        /// <returns>A pointer to the first page on success, null on failure.</returns>
        public static void* AllocPages(PageType aType, uint aPageCount = 1)
        {
            byte* xPos = null;

            // Could combine with an external method or delegate, but will slow things down
            // unless we can force it to be inlined.
            // Allocate single blocks at the bottom, larger blocks at the top to help reduce fragmentation.
            uint xCount = 0;
            if (aPageCount == 1)
            {
                for (byte* p = mRAT; p < mRAT + TotalPageCount; p++)
                {
                    if (*p == (byte)PageType.Empty)
                    {
                        xPos = p;
                        break;
                    }
                }
            }
            else
            {
                // This loop will FAIL if mRAT is ever 0. This should be impossible though
                // so we don't bother to account for such a case. xPos would also have issues.
                for (byte* p = mRAT + TotalPageCount - 1; p >= mRAT; p--)
                {
                    if (*p == (byte)PageType.Empty)
                    {
                        if (++xCount == aPageCount)
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
                var diff = xPos - mRAT;
                byte* xResult = RamStart + diff * PageSize;
                *xPos = (byte)aType;
                for (byte* p = xPos + 1; p < xPos + xCount; p++)
                {
                    *p = (byte)PageType.Extension;
                }
                CPU.ZeroFill((uint)xResult, PageSize * aPageCount);
                return xResult;
            }
            return null;
        }

        /// <summary>
        /// Gets the index in RAT to which the specified pointer belongs.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <returns>The index in RAT to which this pointer belongs.</returns>
        /// <exception cref="Exception">Thrown if the page type is not found.</exception>
        public static uint GetFirstRATIndex(void* aPtr)
        {
            var xPos = (uint)((byte*)aPtr - RamStart) / PageSize;
            // See the note about when mRAT = 0 in Alloc.
            for (byte* p = mRAT + xPos; p >= mRAT; p--)
            {
                if (*p != (byte)PageType.Extension)
                {
                    return (uint)(p - mRAT);
                }
            }
            throw new Exception("Page type not found. Likely RAT is corrupted.");
        }

        /// <summary>
        /// Gets the pointer to the page to which the specified pointer belongs.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <returns>A pointer to the page.</returns>
        public static byte* GetPagePtr(void* aPtr)
        {
            return (byte*)aPtr - ((byte*)aPtr - RamStart) % PageSize;
        }

        /// <summary>
        /// Gets the page type pointed to by a pointer to the RAT entry.
        /// </summary>
        /// <param name="aPtr">A pointer to the page to get the type of.</param>
        /// <returns>The page type.</returns>
        public static PageType GetPageType(void* aPtr)
        {
            if (aPtr < RamStart || aPtr > HeapEnd)
            {
                return PageType.Empty;
            }
            return (PageType)mRAT[GetFirstRATIndex(aPtr)];
        }

        /// <summary>
        /// Frees a page at the specified index.
        /// </summary>
        /// <param name="aPageIdx">The index of the page to be freed.</param>
        public static void Free(uint aPageIdx)
        {
            byte* p = mRAT + aPageIdx;
            *p = (byte)PageType.Empty;
            for (; p < mRAT + TotalPageCount; )
            {
                if (*++p != (byte)PageType.Extension)
                {
                    break;
                }
                *p = (byte)PageType.Empty;
            }
        }
    }
}
