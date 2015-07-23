using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.IOGroup
{
    public class VBE : IOGroup
    {
        public IOPort VBE_DISPI_IOPORT_INDEX = new IOPort(0x01CE);
        public IOPort VBE_DISPI_IOPORT_DATA = new IOPort(0x01CF);
    }
}
