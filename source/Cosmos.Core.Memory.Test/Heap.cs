using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {
  // TODO Does more than just Heap. Separate out RAT etc later.
  unsafe static public class Heap {
    static private Native PtrSize = sizeof(Native);
    // Native Intel page size
    // x86 Page Size: 4k, 2m (PAE only), 4m
    // x64 Page Size: 4k, 2m
    static private Native PageSize = 4096;

    // Start of area usable for heap, and also start of heap.
    static private byte* mStartPtr;
    // Size of heap
    static private Native mSize;
    // Calculated from mSize
    static private Native mPageCount;

    // RAT - RAM Allocation Table (Covers Data area only)
    // We need a pointer as the RAT can move around in future with dynamic RAM etc.
    static private byte* mRatPtr;
    static public class PageType {
      public const byte Empty = 0;
      // Extension of previous page.
      public const byte Extension = 1;
      public const byte RAT = 2;
    }
    
    static public void Init(byte* aStartPtr, Native aSize) {
      if (aSize % PageSize != 0) {
        throw new Exception("Heap size must be page aligned.");
      }

      mStartPtr = aStartPtr;
      mSize = aSize;
      mPageCount = aSize / PageSize;

      InitRAT();
    }
    static private void InitRAT() {
      mRatPtr = mStartPtr;

      // Clear RAT
      for (Native i = 0; i < mPageCount; i++) {
        mRatPtr[i] = PageType.Empty;
      }

      // We need one status byte for each block.
      // Intel blocks are 4k (10 bits). So for 4GB, this means
      // 32 - 12 = 20 bits, 1 MB for a RAT for 4GB. 0.025%
      Native xRatPageCount = mPageCount / PageSize;
      ReservePages(xRatPageCount);
    }

    static public Native GetPageCount(byte aType = 0) {
      Native xResult = 0;
      for (Native i = 0; i < mPageCount; i++) {
        if (mRatPtr[i] == aType) {
          xResult++;
        }
      }
      return xResult;
    }

    static private byte* ReservePages(Native aCount = 1) {
      Native xCount = 0;
      byte* xResult = null;
      for (Native i = 0; i < mPageCount; i++) {
        if (mRatPtr[i] == 0) {
          if (xCount == 0) {
            xResult = mRatPtr + i * PageSize;
          }

          xCount++;
          if (xCount == aCount) {
            return xResult;
          }
        } else {
          xCount = 0;
        }
      }
      return null;
    }

    static public void* New(Native aSize) {
      return null;
    }

    static private void* NewBlock(int aSize) {
      // size is inclusive? final sizse important when we get to vm

      // Block Status - 1 byte of 4
      //    -Has Data
      //    -Empty (Can be removed or merged)
      // Next Block - Pointer to data. 0 if this is current last.
      // Data Size - Native - Size of data, not including header.
      // Data
      return null;
    }
  }
}
