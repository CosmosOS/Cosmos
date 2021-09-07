using System;
using IL2CPU.API.Attribs;
using Cosmos.System;

namespace KernelGen3 {
    static public class Boot {
        [BootEntry]
        static private void Init() {
            Cosmos.System.Boot.TempDebugTest();

            while (true) {
            }
        }
    }
}
