using XSharp.Assembler;
using XSharp.Assembler.x86;
using XSharp;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm.MemoryOperations
{
    public class MemoryOperationsCopy128BytesAsm : AssemblerMethod
    {
        private const int DestDisplacement = 12;
        private const int SrcDisplacement = 8;

        // unsafe private static void Copy128Bytes(byte* dest, byte* src)
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("CALLED!");

            // Copy Src to ESI
            XS.Set(RSI, RBP, sourceIsIndirect: true, sourceDisplacement: SrcDisplacement);
            // Copy Dst to EDI
            XS.Set(RDI, RBP, sourceIsIndirect: true, sourceDisplacement: DestDisplacement);

            // move first 16 bytes of data from src to registers
            XS.SSE.MoveDQU(XMM0, RSI, sourceIsIndirect: true);
            // move second 16 bytes of data from src to registers
            XS.SSE.MoveDQU(XMM1, RSI, sourceIsIndirect: true, sourceDisplacement: 16);
            // move third 16 bytes of data from src to registers
            XS.SSE.MoveDQU(XMM2, RSI, sourceIsIndirect: true, sourceDisplacement: 32);
            // move fourth 16 bytes of data from src to registers
            XS.SSE.MoveDQU(XMM3, RSI, sourceIsIndirect: true, sourceDisplacement: 48);
            // move fourth 16 bytes of data from src to registers
            XS.SSE.MoveDQU(XMM4, RSI, sourceIsIndirect: true, sourceDisplacement: 64);
            XS.SSE.MoveDQU(XMM5, RSI, sourceIsIndirect: true, sourceDisplacement: 80);
            XS.SSE.MoveDQU(XMM6, RSI, sourceIsIndirect: true, sourceDisplacement: 96);
            XS.SSE.MoveDQU(XMM7, RSI, sourceIsIndirect: true, sourceDisplacement: 112);

            // move data from registers to dest
            XS.SSE.MoveDQU(RDI, XMM0, destinationIsIndirect: true);
            XS.SSE.MoveDQU(RDI, XMM1, destinationIsIndirect: true, destinationDisplacement: 16);
            XS.SSE.MoveDQU(RDI, XMM2, destinationIsIndirect: true, destinationDisplacement: 32);
            XS.SSE.MoveDQU(RDI, XMM3, destinationIsIndirect: true, destinationDisplacement: 48);
            XS.SSE.MoveDQU(RDI, XMM4, destinationIsIndirect: true, destinationDisplacement: 64);
            XS.SSE.MoveDQU(RDI, XMM5, destinationIsIndirect: true, destinationDisplacement: 80);
            XS.SSE.MoveDQU(RDI, XMM6, destinationIsIndirect: true, destinationDisplacement: 96);
            XS.SSE.MoveDQU(RDI, XMM7, destinationIsIndirect: true, destinationDisplacement: 112);
        }
    }
}
