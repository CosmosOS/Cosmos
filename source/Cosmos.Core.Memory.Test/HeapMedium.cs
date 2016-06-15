using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test {
  unsafe static public class HeapMedium {
    public const Native PrefixBytes = 4 * sizeof(Native);
  }
}
