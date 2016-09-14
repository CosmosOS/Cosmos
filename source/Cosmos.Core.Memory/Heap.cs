using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory {

  unsafe static public class Heap {
    static public void Init() {
      HeapSmall.Init();
      HeapMedium.Init();
      HeapLarge.Init();
    }

    static public byte* Alloc(Native aSize) {
      if (aSize <= HeapSmall.mMaxItemSize) {
        return HeapSmall.Alloc(aSize);
      } else if (aSize <= HeapMedium.MaxItemSize) {
        return HeapMedium.Alloc(aSize);
      } else {
        return HeapLarge.Alloc(aSize);
      }
    }

    // Keep as void* and not byte* or other. Reduces typecasting from callers
    // who may have typed the pointer to their own needs.
    static public void Free(void* aPtr) {
      //TODO find a better way to remove the double look up here for GetPageType and then again in the
      // .Free methods which actually free the entries in the RAT.
      var xType = RAT.GetPageType(aPtr);
      switch (xType) {
        case RAT.PageType.HeapLarge:
          HeapLarge.Free(aPtr);
          break;

        default:
          throw new Exception("Heap item not found in RAT.");
      }
    }

  }
}
