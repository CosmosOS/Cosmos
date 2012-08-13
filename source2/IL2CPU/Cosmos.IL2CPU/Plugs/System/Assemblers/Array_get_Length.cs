using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.System.Assemblers
{
    public class Array_get_Length: AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
// $this   ebp+8
            new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
            new Push { DestinationIsIndirect = true, DestinationReg = Registers.EAX, DestinationDisplacement = 8 };
        }
    }
}