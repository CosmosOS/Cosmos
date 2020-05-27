using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory {
  unsafe static public class RAT {
    // RAT: RAM Allocation Table
    //
    // A byte table which defines the code which owns the page.
    // Owners can further subdivide table types on their own and RAT
    // code must not assume anything about contents of pages other
    // than who owns them.

    static public class PageType {
      public const byte Empty = 0;

      // Data Types from 1, special meanings from 255 down.
      public const byte RAT = 1;
      public const byte HeapSmall = 2;
      public const byte HeapMedium = 3;
      public const byte HeapLarge = 4;
      // Code
      // Stack
      // Disk Cache

      // Extension of previous page.
      public const byte Extension = 255;
    }

    // Used to bypass certain checks that will fail during tests and debugging.
    static internal bool Debug = false;

    // Native Intel page size
    // x86 Page Size: 4k, 2m (PAE only), 4m
    // x64 Page Size: 4k, 2m
    public const Native PageSize = 4096;

    // Start of area usable for heap, and also start of heap.
    static private byte* mRamStart;
    // Size of heap
    static private Native mRamSize;
    // Calculated from mSize
    static private Native mPageCount;

    // RAT - RAM Allocation Table (Covers Data area only)
    // We need a pointer as the RAT can move around in future with dynamic RAM etc.
    static private byte* mRAT;

    static public void Init(byte* aStartPtr, Native aSize) {
      if ((Native)aStartPtr % PageSize != 0 && !Debug) {
        throw new Exception("RAM start must be page aligned.");
      }

      if (aSize % PageSize != 0) {
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
      for (byte* p = mRAT; p < mRAT + xRatPageBytes - xRatPageCount; p++) {
        *p = PageType.Empty;
      }
      for (byte* p = mRAT + xRatPageBytes - xRatPageCount; p < mRAT + xRatPageBytes; p++) {
        *p = PageType.RAT;
      }

      Heap.Init();
    }

    static public Native GetPageCount(byte aType = 0) {
      Native xResult = 0;
      byte xType = 0; // Could us nullable type instead of this + xCounting, but this is faster.
      bool xCounting = false;
      for (byte* p = mRAT; p < mRAT + mPageCount; p++) {
        if (*p == aType) {
          xType = *p;
          xResult++;
          xCounting = true;
        } else if (xCounting) {
          if (xType == PageType.Extension) {
            xResult++;
          } else {
            xCounting = false;
          }
        }
      }
      return xResult;
    }

    static public void* AllocBytes(byte aType, Native aBytes) {
      return AllocPages(aType, aBytes / PageSize);
    }
    static public void* AllocPages(byte aType, Native aPageCount = 1) {
      byte* xPos = null;

      // Could combine with an external method or delegate, but will slow things down
      // unless we can force it to be inlined.
      //
      // Alloc single blocks at bottom, larger blocks at top to help reduce fragmentation.
      Native xCount = 0;
      if (aPageCount == 1) {
        for (byte* p = mRAT; p < mRAT + mPageCount; p++) {
          if (*p == PageType.Empty) {
            xCount++;
            if (xCount == aPageCount) {
              xPos = p - xCount + 1;
              break;
            }
          } else {
            xCount = 0;
          }
        }
      } else {
        // This loop will FAIL if mRAT is ever 0. This should be impossible though
        // so we don't bother to account for such a case. xPos would also have issues.
        for (byte* p = mRAT + mPageCount - 1; p >= mRAT; p--) {
          if (*p == PageType.Empty) {
            xCount++;
            if (xCount == aPageCount) {
              xPos = p;
              break;
            }
          } else {
            xCount = 0;
          }
        }
      }

      // If we found enough space, mark it as used.
      if (xPos != null) {
        byte* xResult = mRamStart + (xPos - mRAT) * PageSize;
        *xPos = aType;
        for (byte* p = xPos + 1; p < xPos + xCount; p++) {
          *p = PageType.Extension;
        }
        return xResult;
      }

      return null;
    }

    static public Native GetFirstRAT(void* aPtr) {
      var xPos = (Native)((byte*)aPtr - mRamStart) / RAT.PageSize;
      // See note about when mRAT = 0 in Alloc.
      for (byte* p = mRAT + xPos; p >= mRAT; p--) {
        if (*p != PageType.Extension) {
          return (Native)(p - mRAT);
        }
      }
      throw new Exception("Page type not found. Likely RAT is rotten.");
    }

    static public byte GetPageType(void* aPtr) {
      return mRAT[GetFirstRAT(aPtr)];
    }

    static public void Free(Native aPageIdx) {
      byte* p = mRAT + aPageIdx;
      *p = PageType.Empty;
      for (; p < mRAT + mPageCount; p++) {
        if (*p != PageType.Extension) {
          break;
        }
        *p = PageType.Empty;
      }
    }
  }
}
