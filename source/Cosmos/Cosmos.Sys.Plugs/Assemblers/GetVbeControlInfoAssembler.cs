using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;

namespace Cosmos.Sys.Plugs.Assemblers
{
    public class GetVbeControlInfoAssembler: AssemblerMethod
    {
        public override void AssembleNew(object aAssembler, object aMethodInfo)
        {
            new Push { DestinationRef = ElementReference.New("MultibootGraphicsRuntime_VbeControlInfoAddr"), DestinationIsIndirect = true };
        }
    }
}