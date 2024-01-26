using IL2CPU.API;
using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class ArrayClearAsm : AssemblerMethod
    {
        private const int SourceArrayDisplacement = 12;

        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // load element size into eax
            // load length into ebx
            // calculate entire size into eax and move to exc
            // load start into edi
            // clear ecx bytes starting at edi

            // load element size into eax
            XS.Set(RAX, RBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(RAX, 8);
            XS.Set(RAX, RAX, sourceIsIndirect: true);

            // load length into ebx
            XS.Set(RBX, RBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(RBX, 12);
            XS.Set(RBX, RBX, sourceIsIndirect: true);

            // calculate size in bytes and move to ecx
            XS.Multiply(RBX);
            XS.Set(RCX, RAX);

            // load start into esi
            XS.Set(RDI, RBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(RDI, 16);

            // clear eax bytes starting at esi
            XS.Set(RAX, 0);
            XS.LiteralCode("rep stosb");
        }
    }
}
