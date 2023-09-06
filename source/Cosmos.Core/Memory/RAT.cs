using System;
using Cosmos.Debug.Kernel;
using Native = System.UInt32;

namespace Cosmos.Core.Memory
{
    unsafe static public class RAT
    {
        public enum PageType
        {
            Empty = 0,
            GCManaged = 1,
            HeapSmall = 3,
            HeapMedium = 5,
            HeapLarge = 7,
            RAT = 32,
            SMT = 64,
            Extension = 128,
        }

        static internal bool Debug = false;

        public const uint PageSize = 4096;

        public static byte* RamStart;
        public static byte* HeapEnd;
        public static uint RamSize;
        static public uint TotalPageCount;

        public static byte* mRAT;

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

        public static byte* GetPagePtr(void* aPtr)
        {
            return (byte*)aPtr - ((byte*)aPtr - RamStart) % PageSize;
        }

        public static PageType GetPageType(void* aPtr)
        {
            if (aPtr < RamStart || aPtr > HeapEnd)
            {
                return PageType.Empty;
            }
            return (PageType)mRAT[GetFirstRATIndex(aPtr)];
        }

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
