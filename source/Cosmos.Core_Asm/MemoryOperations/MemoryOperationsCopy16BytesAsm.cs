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
            XS.Set(RSI, RBP, sourceIsIndirect: true, sourceDisplacement: SrcDisplacement);
            // Copy Dst to EDI
            XS.Set(RDI, RBP, sourceIsIndirect: true, sourceDisplacement: DestDisplacement);

            // move data from src to registers
            XS.SSE.MoveDQU(XMM0, RSI, sourceIsIndirect: true);

            // move data from registers to dest
            XS.SSE.MoveDQU(RDI, XMM0, destinationIsIndirect: true);
        }
    }
}
