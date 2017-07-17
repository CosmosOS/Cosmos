using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class EnableInterrupts : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Sti();
        }
    }
}
