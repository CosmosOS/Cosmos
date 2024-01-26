using Cosmos.Debug.Kernel;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class CPUReadCPUIDAsm : AssemblerMethod
    {
        private const int CpuIdTypeArgOffset = 24;
        private const int CpuIdEAXAddressArgOffset = 20;
        private const int CpuIdEBXAddressArgOffset = 16;
        private const int CpuIdECXAddressArgOffset = 12;
        private const int CpuIdEDXAddressArgOffset = 8;

        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Set(RAX, RBP, sourceDisplacement: CpuIdTypeArgOffset, sourceIsIndirect: true);
            XS.Cpuid();

            XS.Set(RDI, RBP, sourceDisplacement: CpuIdEAXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(RDI, RAX, destinationIsIndirect: true);

            XS.Set(RDI, RBP, sourceDisplacement: CpuIdEBXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(RDI, RBX, destinationIsIndirect: true);

            XS.Set(RDI, RBP, sourceDisplacement: CpuIdECXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(RDI, RCX, destinationIsIndirect: true);

            XS.Set(RDI, RBP, sourceDisplacement: CpuIdEDXAddressArgOffset, sourceIsIndirect: true);
            XS.Set(RDI, RDX, destinationIsIndirect: true);
        }
    }
}
