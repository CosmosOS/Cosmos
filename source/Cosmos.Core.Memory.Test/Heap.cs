using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {

  unsafe static public class Heap {
  
    static public byte* Alloc(Native aSize) {
      //TODO - Dont use medium if its close to the page size - does'nt make sense to make a medium page
      // with only enough free space for something that would be small anyway.
      if (aSize <= RAT.PageSize - HeapMedium.PrefixBytes) {
        return HeapLarge.Alloc(aSize);
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
