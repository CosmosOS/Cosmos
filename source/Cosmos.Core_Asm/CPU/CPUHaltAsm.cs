using Cosmos.Assembler;
using XSharp.Common;

namespace Cosmos.Core.Plugs_Asm
{
    public class CPUHaltAsm : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Halt();
        }
    }
}
