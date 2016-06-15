using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class Array_get_Length : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            // $this   ebp+8
            new Mov { DestinationReg = RegistersEnum.EAX, SourceReg = RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
            new Mov { DestinationReg = RegistersEnum.EAX, SourceReg = RegistersEnum.EAX, SourceIsIndirect = true };
            new Push { DestinationIsIndirect = true, DestinationReg = RegistersEnum.EAX, DestinationDisplacement = 8 };
        }
    }
}