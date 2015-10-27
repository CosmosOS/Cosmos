using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.IOGroup
{
    public class VBE : IOGroup
    {
        public IOPort VbeIndex= new IOPort(0x01CE);
        public IOPort VbeData = new IOPort(0x01CF);

        public MemoryBlock08 VGAMemoryBlock = new MemoryBlock08(0xE0000000, 1280 * 1024 * 4);
    }
}
