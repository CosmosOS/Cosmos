using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.Assemblers
{
    public class DisableInterruptsAssembler: AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            new ClearInterruptFlag();
        }
    }
}
