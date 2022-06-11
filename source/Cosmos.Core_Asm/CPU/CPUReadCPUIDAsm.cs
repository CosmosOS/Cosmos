using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm;

public class CPUReadCPUIDAsm : AssemblerMethod
{
    private const int CpuIdTypeArgOffset = 24;
    private const int CpuIdEAXAddressArgOffset = 20;
    private const int CpuIdEBXAddressArgOffset = 16;
    private const int CpuIdECXAddressArgOffset = 12;
    private const int CpuIdEDXAddressArgOffset = 8;

    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.Set(EAX, EBP, sourceDisplacement: CpuIdTypeArgOffset, sourceIsIndirect: true);
        XS.Cpuid();

        XS.Set(EDI, EBP, sourceDisplacement: CpuIdEAXAddressArgOffset, sourceIsIndirect: true);
        XS.Set(EDI, EAX, true);

        XS.Set(EDI, EBP, sourceDisplacement: CpuIdEBXAddressArgOffset, sourceIsIndirect: true);
        XS.Set(EDI, EBX, true);

        XS.Set(EDI, EBP, sourceDisplacement: CpuIdECXAddressArgOffset, sourceIsIndirect: true);
        XS.Set(EDI, ECX, true);

        XS.Set(EDI, EBP, sourceDisplacement: CpuIdEDXAddressArgOffset, sourceIsIndirect: true);
        XS.Set(EDI, EDX, true);
    }
}
