using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {
  unsafe static public class HeapSmall {
    public const Native PrefixBytes = 4 * sizeof(Native);
    public const Native MaxItemSize = 1024 - PrefixBytes;

    static public byte* Alloc(Native aSize) {
      return HeapLarge.Alloc(aSize);
    }

    static public void Free(void* aPtr) {
      HeapLarge.Free(aPtr);
    }
  }
}
