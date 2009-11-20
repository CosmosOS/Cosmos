using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.IL2CPU.Assembler;
using CPUAll = Cosmos.IL2CPU;
using CPUx86 = Cosmos.IL2CPU.X86;
using System.Collections.Generic;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class InitFloat : AssemblerMethod
    {
        public override void AssembleNew(object aAssembler, object aMethodInfo)
        {
            new CPUx86.x87.FloatInit();
        }
    }
}
