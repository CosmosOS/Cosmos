using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.IOGroup {
    public class TextScreen : IOGroup {
       public readonly MemoryBlock Memory = new MemoryBlock(0xB8000, 80 * 25 * 2 / 4);
    }
}
