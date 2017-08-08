using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Assembler.x86;
using Cosmos.Assembler;

namespace Cosmos.Sys.Plugs.Assemblers
{
    public class GetVbeControlInfoAssembler: AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Push(Cosmos.Assembler.ElementReference.New("MultibootGraphicsRuntime_VbeControlInfoAddr"), isIndirect: true);
        }
    }
}