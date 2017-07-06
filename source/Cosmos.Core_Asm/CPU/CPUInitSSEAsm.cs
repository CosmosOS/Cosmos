using Cosmos.Assembler;
using XSharp.Common;

namespace Cosmos.Core_Plugs_Asm
{
    public class CPUInitSSEAsm : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.SSE.SSEInit();
        }
    }
}
