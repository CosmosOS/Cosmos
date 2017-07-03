using Cosmos.Assembler;
using XSharp.Common;

namespace Cosmos.Core.Plugs_Asm
{
    public class CPUGetEndOfKernelAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Push("_end_code");
        }
    }
}
