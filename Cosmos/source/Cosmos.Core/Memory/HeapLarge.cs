using System;
using System.Linq;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory {
  unsafe static public class HeapLarge {
    public const Native PrefixBytes = 4 * sizeof(Native);

    static public void Init() {
    }

    static public byte* Alloc(Native aSize) {
      Native xPages = (Native)((aSize + PrefixBytes) / RAT.PageSize) + 1;
      var xPtr = (Native*)RAT.AllocPages(RAT.PageType.HeapLarge, xPages);

      xPtr[0] = xPages * RAT.PageSize - PrefixBytes; // Allocated data size
      xPtr[1] = aSize; // Actual data size
      xPtr[2] = 0; // Ref count
      xPtr[3] = 0; // Ptr to first

      return (byte*)xPtr + PrefixBytes;
    }

    static public void Free(void* aPtr) {
      // TODO - Should check the page type before freeing to make sure it is a Large?
      // or just trust the caller to avoid adding overhead?
      var xPageIdx = RAT.GetFirstRAT(aPtr);
      RAT.Free(xPageIdx);
    }
  }
}
