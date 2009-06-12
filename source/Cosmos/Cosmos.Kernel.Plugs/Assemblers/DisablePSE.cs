using System;

using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Assembler=Indy.IL2CPU.Assembler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class ASMDisablePSE : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.CR4 };
            new And { DestinationReg = Registers.EAX, SourceValue = 0xFFFFFFEF };
            new Move { DestinationReg = Registers.CR4, SourceReg = Registers.EAX };
        }
    }
}