using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs.Assemblers
{
    public class DisableInterruptsAssembler: AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.ClearInterruptFlag();
        }
    }
}
