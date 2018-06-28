using XSharp.Assembler;
using IL2CPU.API;
using XSharp;
using CPUx86 = XSharp.Assembler.x86;

namespace Cosmos.Core_Asm
{
    public class BufferBlockCopyAsm : AssemblerMethod
    {
        private const int SourceArrayDisplacement = 32;
        private const int SourceIndexDisplacement = 24;
        private const int DestinationArrayDisplacement = 20;
        private const int DestinationIndexDisplacement = 12;
        private const int CountDisplacement = 8;

        /*public static void BlockCopy(
         *			Array src, [ebp + 32]
         *			int srcOffset, [ebp + 24]
         *			Array dst, [ebp + 20]
         *			int dstOffset, [ebp + 12]
         *			int count); [ebp + 8]
         */
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("Source array");
            XS.Set(XSRegisters.ESI, XSRegisters.EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(XSRegisters.ESI, ObjectUtils.FieldDataOffset + 4);
            XS.Comment("Source index");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: SourceIndexDisplacement);
            XS.Add(XSRegisters.ESI, XSRegisters.EAX);

            XS.Comment("Destination array");
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(XSRegisters.EDI, ObjectUtils.FieldDataOffset + 4);
            XS.Comment("Destination index");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: DestinationIndexDisplacement);
            XS.Add(XSRegisters.EDI, XSRegisters.EAX);

            XS.Comment("Count");
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: CountDisplacement);
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
