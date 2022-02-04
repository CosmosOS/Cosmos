using Cosmos.Debug.Kernel;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class CPUGetStackStart : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Push("Kernel_Stack");
        }
    }
}
