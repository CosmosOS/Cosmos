using System;
using Cosmos.CPU.x86;
using Sys = Cosmos.System;

namespace KernelGen3 {
    public class Boot : Sys.Boot {

        protected override void Run() {
            Bootstrap.Init();
            Temp.ShowText();

            while (true) {
            }
        }
    }
}
