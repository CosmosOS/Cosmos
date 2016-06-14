using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {
  unsafe static public class RAT {
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
    static public bool Debug = false;

    static private Native PtrSize = sizeof(Native);
    // Native Intel page size
    // x86 Page Size: 4k, 2m (PAE only), 4m
    // x64 Page Size: 4k, 2m
    static public readonly Native PageSize = 4096;

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
      } else if (aSize % PageSize != 0) {
        throw new Exception("RAM size must be page aligned.");
      }

      mRamStart = aStartPtr;
      mRamSize = aSize;
      mPageCount = aSize / PageSize;

      mRAT = mRamStart;
      // Clear RAT
      for (Native i = 0; i < mPageCount; i++) {
        mRAT[i] = PageType.Empty;
      }

      // We need one status byte for each block.
      // Intel blocks are 4k (10 bits). So for 4GB, this means
      // 32 - 12 = 20 bits, 1 MB for a RAT for 4GB. 0.025%
      Native xRatPageCount = mPageCount / PageSize;
      Alloc(PageType.RAT, xRatPageCount);

      Heap.Init();
    }

    static public Native GetPageCount(byte aType = 0) {
      Native xResult = 0;
      bool xCounting = false;
      for (Native i = 0; i < mPageCount; i++) {
        byte xType = mRAT[i];
        if (xType == aType) {
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

    static public byte* Alloc(byte aType, Native aPageCount = 1) {
      Native? xPos = null;

      // Could combine with an external method or delegate, but will slow things down
      // unless we can force it to be inlined.
      //
      // Alloc single blocks at bottom, larger blocks at top to help reduce fragmentation.
      Native xCount = 0;
      if (aPageCount == 1) {
        for (Native i = 0; i < mPageCount; i++) {
          if (mRAT[i] == PageType.Empty) {
            xCount++;
            if (xCount == aPageCount) {
              xPos = i - xCount - 1;
              break;
            }
          } else {
            xCount = 0;
          }
        }
      } else {
        for (Native i = mPageCount - 1; i >= 0; i--) {
          if (mRAT[i] == PageType.Empty) {
            xCount++;
            if (xCount == aPageCount) {
              xPos = i;
              break;
            }
          } else {
            xCount = 0;
          }
        }
      }

      // If we found enough space, mark it as used.
      if (xPos.HasValue) {
        byte* xResult = mRamStart + xPos.Value * PageSize;
        mRAT[xPos.Value] = aType;
        for (Native i = xPos.Value + 1; i < xPos.Value + xCount; i++) {
          mRAT[i] = PageType.Extension;
        }
        return xResult;
      }

      return null;
    }
  }
}
