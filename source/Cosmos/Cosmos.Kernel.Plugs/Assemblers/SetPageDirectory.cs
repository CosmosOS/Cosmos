using System;

using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Assembler=Indy.IL2CPU.Assembler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class ASMSetPageDirectory : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 0x8 };
            new Move { DestinationReg = Registers.CR3, SourceReg = Registers.EAX };
        }
    }
}