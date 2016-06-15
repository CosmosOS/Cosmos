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

    static public byte* Alloc(Native aSize) {
      //TODO - Dont use medium if its close to the page size - does'nt make sense to make a medium page
      // with only enough free space for something that would be small anyway.
      if (aSize <= RAT.PageSize - HeapMedium.PrefixBytes) {
        return HeapLarge.Alloc(aSize);
      } else {
        return HeapLarge.Alloc(aSize);
      }
    }

  }
}
