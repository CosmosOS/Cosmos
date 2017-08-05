using System;
using Cosmos.CPU;
using Sys = Cosmos.System;

namespace KernelGen3 {
    public class Boot : Sys.Boot {

        protected override void Run() {
            Bootstrap.Init();
            Cosmos.CPU.Temp.ShowText();

            while (true) {
            }
        }
    }
}
