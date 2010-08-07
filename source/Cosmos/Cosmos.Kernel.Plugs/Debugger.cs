using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Asm = Cosmos.Compiler.Assembler;
using Cosmos.Compiler.XSharp;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(Cosmos.Debug.Debugger))]
    public class Debugger {
	    [PlugMethod(Assembler = typeof(DebugBreak))]
        public static unsafe void Break() { }

		[PlugMethod(Assembler = typeof(DebugSend))]
        public static unsafe void Send(int aLength, char* aText) { }

        [PlugMethod(Assembler = typeof(DebugSendPtr))]
        public static unsafe void SendPtr(object aPtr) { }

		[PlugMethod(Assembler = typeof(DebugTraceOff))]
        public static void TraceOff() { }

		[PlugMethod(Assembler = typeof(DebugTraceOn))]
        public static void TraceOn() { }
    }

    public class DebuggerAsm : Y86 {
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

        public void SendPtr()
        {
            IfDefined("DEBUGSTUB");
            PushAll32();
            Call("DebugStub_SendPtr");
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
        public override void AssembleNew(object aAssembler, object aMethodInfo) {
            new DebuggerAsm().TraceOff();
        }
    }

    public class DebugTraceOn : AssemblerMethod {
        public override void AssembleNew(object aAssembler, object aMethodInfo) {
            new DebuggerAsm().TraceOn();
        }
    }

    public class DebugBreak : AssemblerMethod {
        public override void AssembleNew(object aAssembler, object aMethodInfo) {
            new DebuggerAsm().Break();
        }
    }

    public class DebugSend : AssemblerMethod {
        public override void AssembleNew(object aAssembler, object aMethodInfo) {
            new DebuggerAsm().SendText();
        }
    }

    public class DebugSendPtr : AssemblerMethod
    {
        public override void AssembleNew(object aAssembler, object aMethodInfo)
        {
            new DebuggerAsm().SendPtr();
        }
    }
}
