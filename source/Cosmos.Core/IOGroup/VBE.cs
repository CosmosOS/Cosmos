using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.IOGroup
{
    public class VBE : IOGroup
    {
        public IOPort VBE_DISPI_INDEX_ENABLE = new IOPort(0x4);
        public IOPort VBE_DISPI_INDEX_XRES = new IOPort(0x1);
        public IOPort VBE_DISPI_INDEX_YRES = new IOPort(0x2);
        public IOPort VBE_DISPI_INDEX_BPP = new IOPort(0x3);
    }
}
