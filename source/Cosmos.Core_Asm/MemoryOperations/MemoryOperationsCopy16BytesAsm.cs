using XSharp.Assembler;
using XSharp.Assembler.x86;
using XSharp;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm.MemoryOperations
{
    public class MemoryOperationsCopy16BytesAsm : AssemblerMethod
    {
        private const int DestDisplacement = 12;
        private const int SrcDisplacement = 8;

        // unsafe private static void Copy16Bytes(byte* dest, byte* src)
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // Copy Src to ESI
            XS.Set(ESI, EBP, sourceIsIndirect: true, sourceDisplacement: SrcDisplacement);
            // Copy Dst to EDI
            XS.Set(EDI, EBP, sourceIsIndirect: true, sourceDisplacement: DestDisplacement);

            // move data from src to registers
            XS.SSE.MoveDQU(XMM0, ESI, sourceIsIndirect: true);

            // move data from registers to dest
            XS.SSE.MoveDQU(EDI, XMM0, destinationIsIndirect: true);
        }
    }
}
