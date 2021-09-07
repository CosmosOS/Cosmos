using XSharp.Assembler;
using XSharp;
using CPUx86 = XSharp.Assembler.x86;

namespace Cosmos.Core_Asm
{
    public class MultibootAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            new CPUx86.Push { DestinationRef = ElementReference.New("MultiBootInfo_Structure"), DestinationIsIndirect = true };
        }
    }
}
