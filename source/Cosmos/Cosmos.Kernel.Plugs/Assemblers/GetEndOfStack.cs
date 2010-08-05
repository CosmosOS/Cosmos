using System;
using System.Collections.Generic;
using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class GetEndOfStack : AssemblerMethod
    {
        public override void AssembleNew(object aAssembler, object aMethodInfo)
        {
            new CPUx86.Push
            {
                DestinationRef = ElementReference.New("Kernel_Stack")
            };
        }
    }
}