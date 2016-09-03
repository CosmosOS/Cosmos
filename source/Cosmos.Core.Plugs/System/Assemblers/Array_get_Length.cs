using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class Array_get_Length : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            // $this   ebp+8
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceDisplacement: 8, sourceIsIndirect: true); // element count
            XS.Push(XSRegisters.EAX);
        }
    }
}
