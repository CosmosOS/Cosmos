using XSharp.Assembler;
using XSharp.Assembler.x86;
using XSharp;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm.MemoryOperations
{
    public class MemoryOperationsCopy128BlocksAsm : AssemblerMethod
    {
        private const int DestDisplacement = 16;
        private const int SrcDisplacement = 12;
        private const int BlocksNumDisplacement = 8;

        // unsafe private static void Copy128Bytes(byte* dest, byte* src, int blocksNum)
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("CALLED!");

            // Copy Src to ESI
            XS.Set(ESI, EBP, sourceIsIndirect: true, sourceDisplacement: SrcDisplacement);
            // Copy Dst to EDI
            XS.Set(EDI, EBP, sourceIsIndirect: true, sourceDisplacement: DestDisplacement);
            // Copy BlocksNum to ECX
            XS.Set(ECX, EBP, sourceDisplacement: BlocksNumDisplacement);

            /* Do the 'loop' */
            XS.Label(".loop");

            // move 128 bytes of src to 8 XMM register

            XS.SSE.MoveDQU(XMM0, ESI, sourceIsIndirect: true);
            XS.SSE.MoveDQU(XMM1, ESI, sourceIsIndirect: true, sourceDisplacement: 16);
            XS.SSE.MoveDQU(XMM2, ESI, sourceIsIndirect: true, sourceDisplacement: 32);
            XS.SSE.MoveDQU(XMM3, ESI, sourceIsIndirect: true, sourceDisplacement: 48);
            XS.SSE.MoveDQU(XMM4, ESI, sourceIsIndirect: true, sourceDisplacement: 64); 
            XS.SSE.MoveDQU(XMM5, ESI, sourceIsIndirect: true, sourceDisplacement: 80);
            XS.SSE.MoveDQU(XMM6, ESI, sourceIsIndirect: true, sourceDisplacement: 96);
            XS.SSE.MoveDQU(XMM7, ESI, sourceIsIndirect: true, sourceDisplacement: 112);

            // move 128 bytes from the 8 XMM registers to dest
            XS.SSE.MoveDQU(EDI, XMM0, destinationIsIndirect: true);
            XS.SSE.MoveDQU(EDI, XMM1, destinationIsIndirect: true, destinationDisplacement: 16);
            XS.SSE.MoveDQU(EDI, XMM2, destinationIsIndirect: true, destinationDisplacement: 32);
            XS.SSE.MoveDQU(EDI, XMM3, destinationIsIndirect: true, destinationDisplacement: 48);
            XS.SSE.MoveDQU(EDI, XMM4, destinationIsIndirect: true, destinationDisplacement: 64);
            XS.SSE.MoveDQU(EDI, XMM5, destinationIsIndirect: true, destinationDisplacement: 80);
            XS.SSE.MoveDQU(EDI, XMM6, destinationIsIndirect: true, destinationDisplacement: 96);
            XS.SSE.MoveDQU(EDI, XMM7, destinationIsIndirect: true, destinationDisplacement: 112);

            XS.Add(ESI, 128);
            XS.Add(EDI, 128);
            XS.Sub(ECX, 1);
  
            XS.Jump(ConditionalTestEnum.NotZero, ".loop");
        }
    }
}
