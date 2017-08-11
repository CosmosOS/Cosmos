using XSharp.Assembler;
using XSharp;

namespace Cosmos.CPU_Asm
{
    public class ArrayGetLengthAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // $this   ebp+8
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceDisplacement: 8, sourceIsIndirect: true); // element count
            XS.Push(XSRegisters.EAX);
        }
    }
}
