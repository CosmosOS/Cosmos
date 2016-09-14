using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory {
  //TODO Remove empty pages as necessary
  unsafe static public class HeapSmall {
    // Keep word aligned for faster access to slots with small data.
    private const Native PrefixBytes = 3 * sizeof(Native);
    private const Native PrefixItemBytes = 3 * sizeof(Native);
    //
    public static Native mMaxItemSize;

    // Size map table
    // Smaller sizes point to next bigger slot
    // Each Page then is linked forward to next for each slot size.
    private static void** mSMT;

    static public void Init() {
      //TODO Adjust for new page and header sizes 
      // 4 slots, ~1k ea
      Native xMaxItemSize = (RAT.PageSize - PrefixBytes) / 4 - PrefixItemBytes;
      // Word align it
      xMaxItemSize = xMaxItemSize / sizeof(Native) * sizeof(Native);
      InitSMT(xMaxItemSize);

      // TODO Change these sizes after further study and also when page size changes. 
      // SMT can be grown as needed. Also can adjust and create new ones dynamicaly as it runs.
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
      mSMT = (void**)RAT.AllocBytes(RAT.PageType.HeapSmall, mMaxItemSize * (Native)sizeof(void*));
    }

    static void CreatePage(Native aItemSize) {
      if (aItemSize == 0) {
        throw new Exception("aItemSize cannot be 0.");
      } else if (aItemSize % sizeof(Native) != 0) {
        throw new Exception("aItemSize must be word aligned.");
      } else if (mMaxItemSize == 0) {
        throw new Exception("SMT is not initialized.");
      } else if (aItemSize > mMaxItemSize) {
        throw new Exception("Cannot allocate more than MaxItemSize in SmallHeap.");
      }

      // Limit to one page so they are allocated low and easy to move around.
      // In future may put some up top and be larger as well.
      Native xPages = 1;
      var xPtr = (Native*)RAT.AllocPages(RAT.PageType.HeapSmall, xPages);

      // Ptr to next page of same size
      xPtr[0] = 0;
      //
      // # of slots. Necessary for future use when more than one page, but used currently as well.
      Native xSlotSize = aItemSize + PrefixItemBytes;
      Native xItemCount = RAT.PageSize * xPages / xSlotSize;
      xPtr[1] = xItemCount;
      //
      // # of free slots
      xPtr[2] = xItemCount;
      //
      xPtr = xPtr + 3;

      for (Native i = 0; i < xItemCount; i++) {
        byte* xSlotPtr = (byte*)xPtr + i * xSlotSize;
        Native* xMetaDataPtr = (Native*)xSlotPtr;
        xMetaDataPtr[0] = 0; // Actual data size. 0 is empty.
        xMetaDataPtr[1] = 0; // Ref count
        xMetaDataPtr[2] = 0; // Ptr to first
      }
    }

    static public byte* Alloc(Native aSize) {
      return HeapLarge.Alloc(aSize);
    }

    static public void Free(void* aPtr) {
      HeapLarge.Free(aPtr);
    }
  }
}
