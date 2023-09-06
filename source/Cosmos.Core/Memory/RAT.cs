using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// RAT (RAM Allocation Table) class.
    /// </summary>
    public unsafe static class RAT
    {
        /// <summary>
        /// PageType enum. Defines the type of a page.
        /// </summary>
        public enum PageType : byte
        {
            /// <summary>
            /// Represents an empty page or an invalid page.
            /// </summary>
            Empty = 0,

            // Data Types from 1, special meanings from 255 down.
            /// <summary>
            /// Indicates that the page contains objects managed by the GC.
            /// </summary>
            GCManaged = 1,
            /// <summary>
            /// Represents a small heap page.
            /// </summary>
            HeapSmall = 3,
            /// <summary>
            /// Represents a medium heap page.
            /// </summary>
            HeapMedium = 5,
            /// <summary>
            /// Represents a large heap page.
            /// </summary>
            HeapLarge = 7,

            /// <summary>
            /// Represents a RAT type page.
            /// </summary>
            RAT = 32,
            /// <summary>
            /// Represents a page that is part of the SMT.
            /// </summary>
            SMT = 64,
            // Extension of previous page.
            /// <summary>
            /// Represents an extension of a pre-existing page.
            /// </summary>
            Extension = 128,
        }

        // Debug flag. Used to bypass certain checks that may fail during tests and debugging.
        static internal bool Debug = false;

        // Native Intel page size.
        public const uint PageSize = 4096;

        public static byte* RamStart;
        public static byte* HeapEnd;
        public static uint RamSize;
        static public uint TotalPageCount;
        public static byte* mRAT;

        /// <summary>
        /// Initializes the RAT.
        /// </summary>
        /// <param name="aStartPtr">A pointer to the start of the heap.</param>
        /// <param name="aSize">The size of the heap in bytes.</param>
        /// <exception cref="Exception">
        /// Thrown if RAM start or size is not page aligned.
        /// </exception>
        public static void Init(byte* aStartPtr, uint aSize)
        {
            if ((uint)aStartPtr % PageSize != 0 && !Debug)
            {
                Debugger.DoSendNumber((uint)aStartPtr % PageSize);
                Debugger.DoBochsBreak();
                throw new Exception("RAM start must be page aligned.");
            }

            if (aSize % PageSize != 0)
            {
                Debugger.DoSendNumber(aSize % PageSize);
                Debugger.SendKernelPanic(11);
                throw new Exception("RAM size must be page aligned.");
            }

            RamStart = aStartPtr;
            RamSize = aSize;
            HeapEnd = aStartPtr + aSize;
            TotalPageCount = aSize / PageSize;

            uint xRatPageCount = (TotalPageCount - 1) / PageSize + 1;
            uint xRatTotalSize = xRatPageCount * PageSize;
            mRAT = RamStart + RamSize - xRatTotalSize;

            for (byte* p = mRAT; p < mRAT + TotalPageCount - xRatPageCount; p++)
            {
                *p = (byte)PageType.Empty;
            }

            for (byte* p = mRAT + TotalPageCount - xRatPageCount; p < mRAT + xRatTotalSize; p++)
            {
                *p = (byte)PageType.RAT;
            }

            Heap.Init();
        }

        /// <summary>
        /// Gets the page count of a specific type, including extension pages.
        /// </summary>
        /// <param name="aType">The type of pages to count.</param>
        /// <returns>The number of pages of the specified type, including extension pages.</returns>
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
        /// Allocates a given number of pages of the same type.
        /// </summary>
        /// <param name="aType">The type of pages to allocate.</param>
        /// <param name="aPageCount">The number of pages to allocate (default = 1).</param>
        /// <returns>A pointer to the first page on success, or null on failure.</returns>
        public static void* AllocPages(PageType aType, uint aPageCount = 1)
        {
            byte* xPos = null;
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
        /// Gets the index in RAT to which a pointer belongs.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <returns>The index in RAT to which the pointer belongs.</returns>
        /// <exception cref="Exception">Thrown if the page type is not found.</exception>
        public static uint GetFirstRATIndex(void* aPtr)
        {
            var xPos = (uint)((byte*)aPtr - RamStart) / PageSize;
            for (byte* p = mRAT + xPos; p >= mRAT; p--)
            {
                if (*p != (byte)PageType.Extension)
                {
                    return (uint)(p - mRAT);
                }
            }
            throw new Exception("Page type not found. Likely RAT is rotten.");
        }

        /// <summary>
        /// Gets the pointer to the page containing a specific pointer.
        /// </summary>
        /// <param name="aPtr">A pointer to the block.</param>
        /// <returns>A pointer to the page containing the specified pointer.</returns>
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
        /// Frees a page.
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
