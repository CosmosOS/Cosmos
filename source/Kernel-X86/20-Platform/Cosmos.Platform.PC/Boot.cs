using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using IL2CPU.API.Attribs;

namespace Cosmos.Platform.PC {
    static public class Boot {
        [BootEntry(20)]
        static private void Init() {
            HAL.Globals.DeviceMgr = new DeviceMgr();
        }
    }
}
