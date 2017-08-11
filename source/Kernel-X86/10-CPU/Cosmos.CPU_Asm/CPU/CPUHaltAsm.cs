using XSharp.Assembler;
using XSharp.Common;

namespace Cosmos.CPU_Asm {
    public class CPUHaltAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Halt();
        }
    }
}
