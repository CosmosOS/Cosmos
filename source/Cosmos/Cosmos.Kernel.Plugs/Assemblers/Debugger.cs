using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using Asm = Indy.IL2CPU.Assembler;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public class DebugAsm : X86.X.Y86 {
        public void TraceOff() {
            PushAll32();
            Call("DebugStub_TraceOff");
            PopAll32();
        }

        public void TraceOn() {
            PushAll32();
            Call("DebugStub_TraceOn");
            PopAll32();
        }

        public void SendText() {
            Call("DebugStub_SendText");
        }
    }

    //TODO: Make a new plug attrib that assembly plug methods dont need
    // an empty stub also, its just extra fluff - although they allow signature matching
    // Maybe could merge this into the same unit as the plug
    public class DebugTraceOff : AssemblerMethod {
        //TODO: Make a new AssemblerMethod option that can use x# more direct somehow
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebugAsm().TraceOff();
        }
    }

    public class DebugTraceOn : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebugAsm().TraceOn();
        }
    }


    public class DebugSend : AssemblerMethod {
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebugAsm().SendText();
        }
    }

}
