using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup {
    public class TextScreen : IOGroup {
        public readonly MemoryBlock Memory = new MemoryBlock(0xB8000, 80 * 25 * 2);
        // These should probably move to a VGA class later, or this class should be remade into a VGA class
        public readonly IOPort MiscOutput = new IOPort(0x03C2);
        public readonly IOPort Idx1 = new IOPort(0x03C4);
        public readonly IOPort Data1 = new IOPort(0x03C5);
        public readonly IOPort Idx2 = new IOPort(0x03CE);
        public readonly IOPort Data2 = new IOPort(0x03CF);
        public readonly IOPort Idx3 = new IOPort(0x03D4);
        public readonly IOPort Data3 = new IOPort(0x03D5);
    }
}
