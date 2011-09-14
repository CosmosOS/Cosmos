using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.IOGroup {
    public class PCI : IOGroup {
        public IOPort ConfigAddressPort = new IOPort(0xCF8);
        public IOPort ConfigDataPort = new IOPort(0xCFC);
    }
}
