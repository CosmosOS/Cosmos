using XSharp.Assembler;
using XSharp;

namespace Cosmos.Core_Asm
{
    public class ArrayGetLengthAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // $this   ebp+8
            XS.Set(XSRegisters.RAX, XSRegisters.RBP, sourceDisplacement: 12);
            XS.Set(XSRegisters.RAX, XSRegisters.RAX, sourceDisplacement: 8, sourceIsIndirect: true); // element count
            XS.Push(XSRegisters.RAX);
        }
    }
}
