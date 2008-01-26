using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    // This class provides boot configurations to be called
    // as the first line from Application code.
    // For now we just have default, but can add others in the future
    public class Boot {
        public static void Default() {
            CPU.Init();
        }
    }
}
