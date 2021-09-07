using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    public class CPUReadModelSpecificRegisterAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            /*
             * ; esi register layout: (mperf_hi, mperf_lo, aperf_hi, aperf_lo)
             * ;
             * ; int* ptr = new int[4];
             * ;
             * lea esi,        ptr  ;equivalent with `mov esi, &ptr`
             * mov ecx,        e7h
             * rdmsr
             * mov [esi + 4],  eax
             * mov [esi],      edx
             * mov ecx,        e8h
             * rdmsr
             * mov [esi + 12], eax
             * mov [esi + 8],  edx
             * xor eax,        eax
             * ret
             */

            //XS.Lea(XSRegisters.ESI, intname);
            //XS.Set(XSRegisters.ECX, 0xe7);
            //XS.Rdmsr();
            //XS.Set(XSRegisters.EAX, XSRegisters.ESI, destinationIsIndirect: true, destinationDisplacement: 4);
            //XS.Set(XSRegisters.EDX, XSRegisters.ESI, destinationIsIndirect: true, destinationDisplacement: 0);
            //XS.Set(XSRegisters.ECX, 0xe8);
            //XS.Rdmsr();
            //XS.Set(XSRegisters.EAX, XSRegisters.ESI, destinationIsIndirect: true, destinationDisplacement: 12);
            //XS.Set(XSRegisters.EDX, XSRegisters.ESI, destinationIsIndirect: true, destinationDisplacement: 8);
            //XS.Xor(XSRegisters.EAX, XSRegisters.EAX);
            //XS.Return();
        }
    }
}
