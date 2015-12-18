using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class Array_get_Length : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            // $this   ebp+8
            new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
            new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.EAX, SourceIsIndirect = true };
            new Push { DestinationIsIndirect = true, DestinationReg = Registers.EAX, DestinationDisplacement = 8 };
        }
    }
}