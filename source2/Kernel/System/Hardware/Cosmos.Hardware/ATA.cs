using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware {
    public class ATA {
        protected Core.IOGroup.PciDevice IO;

        public ATA(Core.IOGroup.PciDevice aIO) {
            IO = aIO;
        }
    }
}
