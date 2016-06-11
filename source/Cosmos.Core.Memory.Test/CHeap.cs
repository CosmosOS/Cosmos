using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {
  unsafe static public class CHeap {
    // Start of area usable for heap, and also start of heap.
    static private byte* mStart;
    // Size of heap
    static private Native mSize;
    //
    static private Native PtrSize = sizeof(Native);
    // Native Intel page size
    static private Native BlockSize = 4096;
    
    static public void Init(byte* aStart, Native aSize) {
      if (aSize % BlockSize != 0) {
        throw new Exception("Heap size must be page aligned.");
      }

      mStart = aStart;
      mSize = aSize;

      // We need one status byte for each block.
      // Intel blocks are 4k (10 bits). So for 4GB, this means
      // 32 - 10 = 22 bits
      Native xPageCount = aSize / BlockSize;
    }

    // RAM Allocation Table (RAT)
    // 0 - Empty
    // 1 - RAT

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
