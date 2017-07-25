using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Sys = Cosmos.System;

/*
 * Beware Demo Kernels are not recompiled when its dependencies changes!
 * To force recompilation right click on on the Cosmos icon of the demo solution and do "Build".
 */
namespace Guess {
    public class GuessOS : Sys.Kernel {

        protected override void Run() {
            Console.WriteLine("Booted Kernel Gen3");
            while (true) {
            }
        }
    }
}
