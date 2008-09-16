using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using Asm = Indy.IL2CPU.Assembler;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public class DebugAsm : X86.X.Y86 {
        public void TraceOff() {
            Call("DebugStub_TraceOff");
        }

        public void TraceOn() {
            Call("DebugStub_TraceOn");
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
        //TODO: Make a new AssemblerMethod option that can use x# more direct somehow
        public override void Assemble(Asm.Assembler aAssembler) {
            new DebugAsm().TraceOn();
        }

    }

}
