using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    public class PCSpeaker : IOGroup
    {
        // IO Port 61, channel 2 gate
        public readonly IOPort Gate = new IOPort(0x61);
        // These two ports are shared with the PIT, so names are the same
        // IO Port 43
        public readonly IOPort CommandRegister = new IOPort(0x43);
        // IO Port 42
        public readonly IOPort Channel2Data = new IOPort(0x42);
    }
}
