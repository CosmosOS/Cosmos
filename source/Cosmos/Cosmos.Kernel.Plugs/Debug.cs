using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(Cosmos.Kernel.Debug))]
    public class Debug {
		[PlugMethod(MethodAssembler = typeof(Assemblers.DebugTraceOff))]
        public static void TraceOff() { }

		[PlugMethod(MethodAssembler = typeof(Assemblers.DebugTraceOn))]
        public static void TraceOn() { }
    }
}
