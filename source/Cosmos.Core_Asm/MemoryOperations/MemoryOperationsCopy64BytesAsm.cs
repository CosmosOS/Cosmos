using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm.MemoryOperations;

public class MemoryOperationsCopy64BytesAsm : AssemblerMethod
{
    private const int DestDisplacement = 12;
    private const int SrcDisplacement = 8;

    // unsafe private static void Copy64Bytes(byte* dest, byte* src)
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.Comment("CALLED!");

        // Copy Src to ESI
        XS.Set(ESI, EBP, sourceIsIndirect: true, sourceDisplacement: SrcDisplacement);
        // Copy Dst to EDI
        XS.Set(EDI, EBP, sourceIsIndirect: true, sourceDisplacement: DestDisplacement);

        // move 128 bytes of src to 8 XMM register
        // move first 16 bytes of data from src to registers
        XS.SSE.MoveDQU(XMM0, ESI, sourceIsIndirect: true);
        // move second 16 bytes of data from src to registers
        XS.SSE.MoveDQU(XMM1, ESI, sourceIsIndirect: true, sourceDisplacement: 16);
        // move third 16 bytes of data from src to registers
        XS.SSE.MoveDQU(XMM2, ESI, sourceIsIndirect: true, sourceDisplacement: 32);
        // move fourth 16 bytes of data from src to registers
        XS.SSE.MoveDQU(XMM3, ESI, sourceIsIndirect: true, sourceDisplacement: 48);

        // move data from registers to dest
        XS.SSE.MoveDQU(EDI, XMM0, true);
        XS.SSE.MoveDQU(EDI, XMM1, true, 16);
        XS.SSE.MoveDQU(EDI, XMM2, true, 32);
        XS.SSE.MoveDQU(EDI, XMM3, true, 48);
    }
}
