using System;
using System.Text;
using IL2CPU.API.Attribs;

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
