using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory
{

    unsafe static public class Heap
    {
        public const Native PrefixBytes = 4 * sizeof(Native);

        static public void Init()
        {
            HeapSmall.Init();
            HeapMedium.Init();
            HeapLarge.Init();
        }

        static public byte* Alloc(Native aSize)
        {
            byte xType;

            if (aSize <= HeapSmall.mMaxItemSize)
            {
                xType = RAT.PageType.HeapSmall;
            }
            else if (aSize <= HeapMedium.MaxItemSize)
            {
                xType = RAT.PageType.HeapMedium;
            }
            else
            {
                xType = RAT.PageType.HeapLarge;
            }

            Native xPages = (Native)((aSize + PrefixBytes) / RAT.PageSize) + 1;
            var xPtr = (Native*)RAT.AllocPages(xType, xPages);

            xPtr[0] = xPages * RAT.PageSize - PrefixBytes; // Allocated data size
            xPtr[1] = aSize; // Actual data size
            xPtr[2] = 0; // Ref count
            xPtr[3] = 0; // Ptr to first

            return (byte*)xPtr + PrefixBytes;
        }

        // Keep as void* and not byte* or other. Reduces typecasting from callers
        // who may have typed the pointer to their own needs.
        static public void Free(void* aPtr)
        {
            var xPageIdx = RAT.GetFirstRAT(aPtr);
            RAT.Free(xPageIdx);
        }
    }
}
