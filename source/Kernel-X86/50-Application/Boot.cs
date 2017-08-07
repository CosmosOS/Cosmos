using System;

using Cosmos.IL2CPU.API.Attribs;
using Sys = Cosmos.System;

namespace KernelGen3 {
    static public class Boot {
        [BootEntry]
        static private void Init() {
            // temp
            Cosmos.Platform.PC.Boot.Init();
            // temp
            Cosmos.System.Boot.Init();

            Cosmos.Platform.PC.Debug.ShowText();

            while (true) {
            }
        }
    }
}
