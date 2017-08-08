using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.System {
    static public class Boot {
        [BootEntry(40)]
        static private void Init() {
        }

        static public void TempDebugTest() {
            HAL.Globals.DeviceMgr.Processor.SetOption(1);
        }
    }
}
