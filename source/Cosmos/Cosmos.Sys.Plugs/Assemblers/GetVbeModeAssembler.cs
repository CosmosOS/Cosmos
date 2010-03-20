using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;

namespace Cosmos.Sys.Plugs.Assemblers
{
    public class GetVbeModeAssembler: AssemblerMethod
    {
        public override void AssembleNew(object aAssembler, object aMethodInfo)
        {
            new Xor { DestinationReg = Registers.EAX, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.AX, SourceRef = ElementReference.New("MultibootGraphicsRuntime_VbeMode"), SourceIsIndirect = true };
            new Push { DestinationReg = Registers.EAX };
        }
    }
}