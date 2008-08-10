using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    
    public class Halt : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
            new CPUx86.Halt();
        }
    }
}
