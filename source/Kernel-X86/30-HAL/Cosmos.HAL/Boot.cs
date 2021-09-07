using System;
using System.Collections.Generic;
using System.Text;
using IL2CPU.API.Attribs;

namespace Cosmos.HAL {
    static public class Boot {
        [BootEntry(30)]
        static private void Init() {
        }
    }
}
