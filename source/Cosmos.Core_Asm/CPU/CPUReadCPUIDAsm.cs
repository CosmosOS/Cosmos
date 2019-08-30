using Cosmos.Debug.Kernel;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class CPUReadCPUIDAsm : AssemblerMethod
    {
        private const int CpuIdTypeArgOffset = 4;
        private const int CpuIdEAXAddressArgOffset = 8;
        private const int CpuIdEBXAddressArgOffset = 12;
        private const int CpuIdECXAddressArgOffset = 16;
        private const int CpuIdEDXAddressArgOffset = 20;
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("Load type arg");
            XS.Set(EAX, EBP, sourceDisplacement: CpuIdTypeArgOffset, sourceIsIndirect: true);
            XS.Cpuid();

            XS.Comment("Save eax result arg");
            XS.Set(EDI, EBP, sourceDisplacement: CpuIdEAXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(EDI, EAX, destinationIsIndirect: true);

            XS.Comment("Save ebx result arg");
            XS.Set(EDI, EBP, sourceDisplacement: CpuIdEBXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(EDI, EBX, destinationIsIndirect: true);

            XS.Comment("Save ecx result arg");
            XS.Set(EDI, EBP, sourceDisplacement: CpuIdECXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(EDI, ECX, destinationIsIndirect: true);

            XS.Comment("Save edx result arg");
            XS.Set(EDI, EBP, sourceDisplacement: CpuIdEDXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(EDI, EDX, destinationIsIndirect: true);
        }
    }
}
