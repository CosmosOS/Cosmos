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
