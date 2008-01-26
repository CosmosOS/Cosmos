using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs.Hardware.PC {
    [Plug(Target = typeof(Cosmos.Hardware.PC.Processor))]
    class Processor {
        [PlugMethod(MethodAssembler = typeof(Assemblers.CreateGDT))]
        public static void CreateGDT() { }
    }
}
