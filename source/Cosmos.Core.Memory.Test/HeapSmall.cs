using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {
  //TODO Remove empty pages as necessary
  unsafe static public class HeapSmall {
    public const Native PrefixBytes = 4 * sizeof(Native);
    public static Native mMaxItemSize; 

    private static void** mSMT;

    static public void Init() {
      // Size map table
      // Smaller sizes point to bigger one
      // Each Page then is linked forward to next for each size.

      // //TODO Adjust for new page and header sizes
      InitSMT(1016);

      // TODO Change these sizes after further stufy and also when page size
      // changes. Also can adjust and create new ones dynamicaly as it runs. 
      // SMT can be grown as needed.
      CreatePage(16);
      CreatePage(24);
      CreatePage(48);
      CreatePage(64);
      CreatePage(128);
      CreatePage(256);
      CreatePage(512);
      CreatePage(mMaxItemSize);
    }

    static void InitSMT(Native aMaxItemSize) {
      mMaxItemSize = aMaxItemSize;
      Native xPageCount = mMaxItemSize * (Native)sizeof(void*) / RAT.PageSize;
      mSMT = (void**)RAT.Alloc(RAT.PageType.HeapSmall, xPageCount);
    }

    static void CreatePage(Native aItemSize) {
      if (aItemSize == 0) {
        throw new Exception("ItemSize cannot be 0.");
      } else if (aItemSize % sizeof(Native) != 0) {
        throw new Exception("Item size must be word aligned.");
      } else if (mMaxItemSize == 0) {
        throw new Exception("SMT is not initialized.");
      } else if (aItemSize > mMaxItemSize) {
        throw new Exception("Cannot allocate more than MaxItemSize in SmallHeap.");
      }

      var xPtr = (Native*)RAT.Alloc(RAT.PageType.HeapSmall);
      *xPtr = 0;
      for (void** p = mSMT + aItemSize; ; p--) {
      // TODO - Make a free list, put a ptr in item header and first page header - how to keep compact?
      }
      // Header
      // Ptr to next page of same size

      // Each Item
      //xPtr[0] = aSize; // Actual data size
      //xPtr[1] = 0; // Ref count
      //xPtr[2] = 0; // Ptr to first
    }

    static public byte* Alloc(Native aSize) {
      return HeapLarge.Alloc(aSize);
    }

    static public void Free(void* aPtr) {
      HeapLarge.Free(aPtr);
    }
  }
}
