using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    public class Mouse : IOGroup
    {
        public readonly IOPort p60 = new IOPort(0x60);
        public readonly IOPort p64 = new IOPort(0x64);
    }
}
