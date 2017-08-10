using Cosmos.Assembler;
using XSharp.Common;

namespace Cosmos.Core_Asm
{
    public class CPUInitFloatAsm : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.FPU.FloatInit();
        }
    }
}
