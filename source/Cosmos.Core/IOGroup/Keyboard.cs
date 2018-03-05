using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup {
    public class Keyboard : IOGroup {
        public readonly IOPort Port60 = new IOPort(0x60);
    }
}
