using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.IOGroup {
    public class PIC {
        public readonly IOPort PortCmd1 = new IOPort(0x20);
        public readonly IOPort PortData1 = new IOPort(0x21);
        public readonly IOPort PortCmd2 = new IOPort(0xA0);
        public readonly IOPort PortData2 = new IOPort(0xA1);
    }
}
