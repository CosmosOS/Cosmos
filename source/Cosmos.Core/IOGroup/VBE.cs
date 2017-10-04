using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    public class VBE : IOGroup
    {
        public IOPort VbeIndex= new IOPort(0x01CE);
        public IOPort VbeData = new IOPort(0x01CF);

        /*
         * This not a lot optimal as we are taking a lot of memory and then maybe the driver is configured to go at 320*240!
         */
        public MemoryBlock LinearFrameBuffer = new MemoryBlock(0xE0000000, 1920 * 1200 * 4);
        //public MemoryBlock LinearFrameBuffer = new MemoryBlock(0xE0000000, 1024 * 768 * 4);
    }
}
