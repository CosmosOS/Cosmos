using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC {
    public class Processor : Cosmos.Hardware.Processor {
        public Processor() {
            Processor.CreateGDT();
        }

        // Plugged
        public static void CreateGDT() { }
    }
}
