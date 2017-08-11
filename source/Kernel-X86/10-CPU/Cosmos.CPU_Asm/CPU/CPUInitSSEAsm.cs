using XSharp.Assembler;
using XSharp;

namespace Cosmos.CPU_Asm {
    public class CPUInitSSEAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.SSE.SSEInit();
        }
    }
}
