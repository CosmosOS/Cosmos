using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {

  unsafe static public class Heap {
    static public void Init() {
    }

    static public void* New(Native aSize) {
      return null;
    }

    static private void* NewBlock(Native aSize) {
      return NewBlockLarge(aSize);
    }

    static private void* NewBlockLarge(Native aSize) {
      const Native xPrefixWordsLarge = 4;
      const Native xPrefixSizeLarge = xPrefixWordsLarge * sizeof(Native);

      Native xPages = (Native)((aSize + xPrefixSizeLarge) / RAT.PageSize);
      var xPtr = (Native*)RAT.Alloc(RAT.PageType.HeapLarge, xPages);

      xPtr[0] = xPages * RAT.PageSize - xPrefixSizeLarge; // Allocated data size
      xPtr[1] = aSize; // Actual data size
      xPtr[2] = 0; // Ref count
      xPtr[3] = 0; // Ptr to first

      return xPtr + xPrefixWordsLarge;
    }
  }
}
