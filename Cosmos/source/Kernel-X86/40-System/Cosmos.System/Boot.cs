using System;
using System.Text;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.System {
    static public class Boot {
        [BootEntry(40)]
        static private void Init() {
        }

        static public void TempDebugTest() {
            Cosmos.CPU.x86.TempDebug.ShowText('B');
            HAL.Globals.DeviceMgr.Processor.SetOption(1);
        }
    }
}
