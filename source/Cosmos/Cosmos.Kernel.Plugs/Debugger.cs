using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using Asm = Indy.IL2CPU.Assembler;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(Cosmos.Debug.Debugger))]
    public class Debugger {
	    [PlugMethod(Assembler = typeof(DebugBreak))]
        public static unsafe void Break() { }

		[PlugMethod(Assembler = typeof(DebugSend))]
        public static unsafe void Send(int aLength, char* aText) { }

		[PlugMethod(Assembler = typeof(DebugTraceOff))]
        public static void TraceOff() { }

		[PlugMethod(Assembler = typeof(DebugTraceOn))]
        public static void TraceOn() { }
    }

    public class DebuggerAsm : X86.X.Y86 {
        public void Break() {
            IfDefined("DEBUGSTUB");
            Memory["DebugBreakOnNextTrace", 32] = 1;
            EndIfDefined(); // DEBUGSTUB
        }

        public void SendText() {
            IfDefined("DEBUGSTUB");
            PushAll32();
            Call("DebugStub_SendText");
            PopAll32();
            EndIfDefined(); // DEBUGSTUB
        }

        public void TraceOff() {
            IfDefined("DEBUGSTUB");
            PushAll32();
            Call("DebugStub_TraceOff");
            PopAll32();
            EndIfDefined(); // DEBUGSTUB
        }

        public void TraceOn() {
            IfDefined("DEBUGSTUB");
            PushAll32();
            Call("DebugStub_TraceOn");
            PopAll32();
            EndIfDefined(); // DEBUGSTUB
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

        public override void AssembleNew(object aAssembler) {
          throw new NotImplementedException();
        }
    }

    public class DebugTraceOn : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebuggerAsm().TraceOn();
        }
        public override void AssembleNew(object aAssembler) {
          throw new NotImplementedException();
        }
    }

    public class DebugBreak : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebuggerAsm().Break();
        }
        public override void AssembleNew(object aAssembler) {
          throw new NotImplementedException();
        }
    }

    public class DebugSend : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebuggerAsm().SendText();
        }
        public override void AssembleNew(object aAssembler) {
          throw new NotImplementedException();
        }
    }
}
