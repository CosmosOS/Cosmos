using System;

using Cosmos.IL2CPU.API.Attribs;
using Sys = Cosmos.System;

namespace KernelGen3 {
    public class Boot : Sys.BootOld {
        [BootEntry]
        protected override void Run() {
            // temp
            Cosmos.Platform.PC.Boot.Init();
            // temp
            //Cosmos.System.Boot.Init();

            Cosmos.Platform.PC.Debug.ShowText();

            while (true) {
            }
        }
    }
}
