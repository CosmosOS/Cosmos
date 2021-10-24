using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;
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
        public const uint PageSize = 4096;

        /// <summary>
        /// Start of area usable for heap, and also start of heap.
        /// </summary>
        static private byte* mRamStart;
        /// <summary>
        /// Size of heap.
        /// </summary>
        static private uint mRamSize;
        /// <summary>
        /// Number of pages in the heap.
        /// </summary>
        /// <remarks>Calculated from mSize.</remarks>
        static public uint TotalPageCount;

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
        public static void Init(byte* aStartPtr, uint aSize)
        {
            Debugger.DoSendNumber((uint)aStartPtr);
            Debugger.DoSendNumber(aSize);
            CPU.ZeroFill((uint)aStartPtr, aSize);
            if ((uint)aStartPtr % PageSize != 0 && !Debug)
            {
                Debugger.DoSendNumber(((uint)aStartPtr % PageSize));
                Debugger.DoBochsBreak();
                throw new Exception("RAM start must be page aligned.");
            }

            if (aSize % PageSize != 0)
            {
                Debugger.DoSendNumber((aSize % PageSize));
                Debugger.SendKernelPanic(11);
                throw new Exception("RAM size must be page aligned.");
            }

            mRamStart = aStartPtr;
            mRamSize = aSize;
            TotalPageCount = aSize / PageSize;

            // We need one status byte for each block.
            // Intel blocks are 4k (10 bits). So for 4GB, this means
            // 32 - 12 = 20 bits, 1 MB for a RAT for 4GB. 0.025%
            uint xRatPageCount = (TotalPageCount - 1) / PageSize + 1;
            uint xRatTotalSize = xRatPageCount * PageSize;
            mRAT = mRamStart + mRamSize - xRatTotalSize;

            // Mark empty pages as such in the RAT Table
            for (byte* p = mRAT; p < mRAT + TotalPageCount - xRatPageCount; p++)
            {
                *p = PageType.Empty;
            }
            // Mark the rat pages as such
            for (byte* p = mRAT + TotalPageCount - xRatPageCount; p < mRAT + xRatTotalSize; p++)
            {
                *p = PageType.RAT;
            }
            
            Heap.Init();
        }

        /// <summary>
        /// Get page count.
        /// </summary>
        /// <param name="aType">A page type to count.</param>
        /// <returns>Number of pages of this type including extension pages</returns>
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
                    if (*p == PageType.Extension)
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
        /// Alloc a given number of pages, all of the same type.
        /// </summary>
        /// <param name="aType">A type of pages to alloc.</param>
        /// <param name="aPageCount">Number of pages to alloc. (default = 1)</param>
        /// <returns>A pointer to the first page on success, null on failure.</returns>
        public static void* AllocPages(byte aType, uint aPageCount = 1)
        {
            byte* xPos = null;
            
            // Could combine with an external method or delegate, but will slow things down
            // unless we can force it to be inlined.
            // Alloc single blocks at bottom, larger blocks at top to help reduce fragmentation.
            uint xCount = 0;
            if (aPageCount == 1)
            {
                for (byte* p = mRAT; p < mRAT + TotalPageCount; p++)
                {
                    if (*p == PageType.Empty)
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
                    if (*p == PageType.Empty)
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
                byte* xResult = mRamStart + diff * PageSize;
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
        /// <returns>The index in RAT.</returns>
        /// <exception cref="Exception">Thrown if page type is not found.</exception>
        public static uint GetFirstRAT(void* aPtr)
        {
            var xPos = (uint)((byte*)aPtr - mRamStart) / PageSize;
            // See note about when mRAT = 0 in Alloc.
            for (byte* p = mRAT + xPos; p >= mRAT; p--)
            {
                if (*p != PageType.Extension)
                {
                    return (uint)(p - mRAT);
                }
            }
            throw new Exception("Page type not found. Likely RAT is rotten.");
        }

        public static byte* GetPagePtr(void* aPtr)
        {
            return (byte*)aPtr - ((byte*)aPtr - mRamStart) % PageSize;
        }

        /// <summary>
        /// Get the page type pointed by a pointer to the RAT entry.
        /// </summary>
        /// <param name="aPtr">A pointer to the page to get the type of.</param>
        /// <returns>byte value.</returns>
        /// <exception cref="Exception">Thrown if page type is not found.</exception>
        public static byte GetPageType(void* aPtr)
        {
            return mRAT[GetFirstRAT(aPtr)];
        }

        /// <summary>
        /// Free page.
        /// </summary>
        /// <param name="aPageIdx">A index to the page to be freed.</param>
        public static void Free(uint aPageIdx)
        {
            byte* p = mRAT + aPageIdx;
            *p = PageType.Empty;
            for (; p < mRAT + TotalPageCount; )
            {
                if (*++p != PageType.Extension)
                {
                    break;
                }
                *p = PageType.Empty;
            }
        }
    }
}
