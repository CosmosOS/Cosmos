using XSharp.Assembler;
using IL2CPU.API;
using XSharp;
using CPUx86 = XSharp.Assembler.x86;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm.GCImplementation
{
    class GetPointerAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // we get the object as an object size 4 and we just leave it as a uint*
            // so this is just an illegal cast
            XS.Set(EAX, EBP, sourceDisplacement: 12);
            XS.Push(EAX);
        }
    }
}
