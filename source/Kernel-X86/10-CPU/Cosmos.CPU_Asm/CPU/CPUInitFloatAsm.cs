using XSharp.Assembler;
using XSharp;

namespace Cosmos.CPU_Asm {
    public class CPUInitFloatAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.FPU.FloatInit();
        }
    }
}
