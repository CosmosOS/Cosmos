using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory {
  unsafe static public class HeapMedium {
    public const Native PrefixBytes = 4 * sizeof(Native);
    // TODO Adjust when page size changes from 4k to 2/4mb
    // Also adjust according to heap stats and final adjustments to small heap. ie the -1024 should be at least size of
    // max small heap else it will never get used;
    // HeapMedium may be of limited use with 4k pages depending on the final sizes of the small heap.
    public const Native MaxItemSize = RAT.PageSize - 1024;

    static public void Init() {
    }

    static public byte* Alloc(Native aSize) {
      return HeapLarge.Alloc(aSize);
    }

    static public void Free(void* aPtr) {
      HeapLarge.Free(aPtr);
    }
  }
}
