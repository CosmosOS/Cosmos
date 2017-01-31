using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs.Asm
{
    public class CPUDisableINTsAsm : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.ClearInterruptFlag();
        }
    }
}
