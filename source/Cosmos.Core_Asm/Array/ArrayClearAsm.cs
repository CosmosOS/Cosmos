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
            XS.Set(EAX, EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(EAX, 8);
            XS.Set(EAX, EAX, sourceIsIndirect: true);

            // load length into ebx
            XS.Set(EBX, EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(EBX, 12);
            XS.Set(EBX, EBX, sourceIsIndirect: true);

            // calculate size in bytes and move to ecx
            XS.Multiply(EBX);
            XS.Set(ECX, EAX);

            // load start into esi
            XS.Set(EDI, EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(EDI, 16);

            // clear eax bytes starting at esi
            XS.Set(EAX, 0);
            XS.LiteralCode("rep stosb");
        }
    }
}
