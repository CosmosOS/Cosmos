using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.Platform.PC {
    static public class Boot {
        // [BootEntry(20)]
        static public void Init() {
            // Temp
            CPU.x86.Boot.Init();

            HAL.Globals.Devices = new Devices();
        }
    }
}
