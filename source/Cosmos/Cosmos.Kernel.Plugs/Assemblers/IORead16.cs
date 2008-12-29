using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public sealed class IORead16 : AssemblerMethod {
        public override void Assemble(Assembler aAssembler) {
            //TODO: This is a lot of work to read a port. We need to have some kind of inline ASM option that can emit a single out instruction
            //TODO: Also make an attribute that forces normal inlining fo a method
            new CPUx86.Move { DestinationReg = Registers.EDX, SourceReg = Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x8 };
            //TODO: Do we need to clear rest of EAX first?
			//    MTW: technically not, as in other places, it _should_ be working with AL too..
            new CPUx86.Move { DestinationReg = Registers.EAX, SourceValue = 0 };
            new CPUx86.In { DestinationReg = Registers.AX};
			new CPUx86.Push { DestinationReg = Registers.EAX };
        }
    }
}