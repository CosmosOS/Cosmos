using System;

using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Assembler=Indy.IL2CPU.Assembler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class ASMInvalidatePage : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 0xC };
            //TODO: new InvalPG { DestinationReg = Registers.EAX };
        }
    }
}