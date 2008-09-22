using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using Asm = Indy.IL2CPU.Assembler;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(Cosmos.Debug.Debugger))]
    public class Debugger {
	    [PlugMethod(MethodAssembler = typeof(DebugBreak))]
        public static unsafe void Break() { }

		[PlugMethod(MethodAssembler = typeof(DebugSend))]
        public static unsafe void Send(int aLength, char* aText) { }

		[PlugMethod(MethodAssembler = typeof(DebugTraceOff))]
        public static void TraceOff() { }

		[PlugMethod(MethodAssembler = typeof(DebugTraceOn))]
        public static void TraceOn() { }
    }

    public class DebuggerAsm : X86.X.Y86 {
        public void Break() {
            Quote("%ifdef DEBUGSTUB");
            Memory["DebugBreakOnNextTrace", 32] = 1;
            Quote("%endif");
        }

        public void SendText() {
            Quote("%ifdef DEBUGSTUB");
            PushAll32();
            Call("DebugStub_SendText");
            PopAll32();
            Quote("%endif");
        }

        public void TraceOff() {
            Quote("%ifdef DEBUGSTUB");
            PushAll32();
            Call("DebugStub_TraceOff");
            PopAll32();
            Quote("%endif");
        }

        public void TraceOn() {
            Quote("%ifdef DEBUGSTUB");
            PushAll32();
            Call("DebugStub_TraceOn");
            PopAll32();
            Quote("%endif");
        }
    }

    //TODO: Make a new plug attrib that assembly plug methods dont need
    // an empty stub also, its just extra fluff - although they allow signature matching
    // Maybe could merge this into the same unit as the plug
    public class DebugTraceOff : AssemblerMethod {
        //TODO: Make a new AssemblerMethod option that can use x# more direct somehow
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebuggerAsm().TraceOff();
        }
    }

    public class DebugTraceOn : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebuggerAsm().TraceOn();
        }
    }

    public class DebugBreak : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebuggerAsm().Break();
        }
    }

    public class DebugSend : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebuggerAsm().SendText();
        }
    }
}
