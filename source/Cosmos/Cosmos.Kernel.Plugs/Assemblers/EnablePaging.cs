using System;

using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Assembler=Indy.IL2CPU.Assembler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class ASMEnablePaging : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.CR0 };
            new Or { DestinationReg = Registers.EAX, SourceValue = 0x80000000 };
            new Move { DestinationReg = Registers.CR0, SourceReg = Registers.EAX };
        }
    }
}