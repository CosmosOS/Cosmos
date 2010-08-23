using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.IOGroup {
    public class PIT : IOGroup {
        public readonly IOPort Port40 = new IOPort(0x40);
        public readonly IOPort Port43 = new IOPort(0x43);
        public readonly IOPort Port61 = new IOPort(0x61);
    }
}
