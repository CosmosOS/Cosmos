using System;
using System.Collections.Generic;
using System.Text;

using Cosmos.HAL;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.System {
    static public class Boot {
        //[BootEntry(false, 40)]
        static public void Init() {
            // Temp
            HAL.Boot.Init();
        }
    }
}
