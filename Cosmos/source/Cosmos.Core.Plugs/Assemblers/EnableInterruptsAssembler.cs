using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.Assemblers
{
    public class EnableInterruptsAssembler: AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            new Assembler.x86.Sti();
        }
    }
}
