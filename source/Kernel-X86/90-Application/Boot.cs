using System;
using Cosmos.CPU;
using Sys = Cosmos.System;

 // Beware Demo Kernels are not recompiled when its dependencies changes!
 // To force recompilation right click on on the Cosmos icon of the demo solution and do "Build".
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
