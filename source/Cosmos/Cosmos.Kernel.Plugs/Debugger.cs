using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(Cosmos.Debug.Debugger))]
    public class Debugger {
		[PlugMethod(MethodAssembler = typeof(Assemblers.DebugBreak))]
        public static unsafe void Break() { }

		[PlugMethod(MethodAssembler = typeof(Assemblers.DebugSend))]
        public static unsafe void Send(int aLength, char* aText) { }

		[PlugMethod(MethodAssembler = typeof(Assemblers.DebugTraceOff))]
        public static void TraceOff() { }

		[PlugMethod(MethodAssembler = typeof(Assemblers.DebugTraceOn))]
        public static void TraceOn() { }
    }
}
