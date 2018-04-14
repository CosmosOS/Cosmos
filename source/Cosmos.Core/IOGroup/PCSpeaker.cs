using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    public class PCSpeaker : IOGroup
    {
        public readonly IOPort Port61 = new IOPort(0x61);
        public readonly IOPort Port43 = new IOPort(0x43);
        public readonly IOPort Port42 = new IOPort(0x42);
    }
}
