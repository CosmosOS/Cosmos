using XSharp.Assembler;
using XSharp.Common;

namespace Cosmos.CPU_Asm {
    public class CPUEnableINTsAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.EnableInterrupts();
        }
    }
}
