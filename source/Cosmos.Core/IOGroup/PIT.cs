using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup {
    public class PIT : IOGroup {
        public readonly IOPort Data0 = new IOPort(0x40);
        public readonly IOPort Data1 = new IOPort(0x41);
        public readonly IOPort Data2 = new IOPort(0x42);
        public readonly IOPortWrite Command = new IOPortWrite(0x43);
    }
}
