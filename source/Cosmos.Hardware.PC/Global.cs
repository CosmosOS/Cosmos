using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC {
    public class Global : Cosmos.Hardware.Global {
        public static void Init() {
            mProcessor = new Processor();
        }
    }
}
