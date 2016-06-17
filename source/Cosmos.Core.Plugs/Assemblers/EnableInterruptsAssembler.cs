using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs.Assemblers
{
    public class EnableInterruptsAssembler: AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.EnableInterrupts();
        }
    }
}
