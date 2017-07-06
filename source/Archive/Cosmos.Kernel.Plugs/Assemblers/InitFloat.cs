using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Assembler;
using CPUAll = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using System.Collections.Generic;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class InitFloat : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.FloatInit();
        }
    }
}
