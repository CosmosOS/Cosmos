using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using ObjectInfo = Cosmos.IL2CPU.Plugs.System.ObjectImpl;

namespace Cosmos.IL2CPU.Plugs.Assemblers.Buffer
{
    public class BufferBlockCopyAsm : AssemblerMethod
    {
        private const int SourceArrayDisplacement = 32;
        private const int SourceIndexDisplacement = 24;
        private const int DestinationArrayDisplacement = 20;
        private const int DestinationIndexDisplacement = 12;
        private const int CountDisplacement = 8;

        /*public static void BlockCopy(
         *			Array src, [ebp + 24]
         *			int srcOffset, [ebp + 20]
         *			Array dst, [ebp + 16]
         *			int dstOffset, [ebp + 12]
         *			int count); [ebp + 8]
         */
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("Source array");
            XS.Set(XSRegisters.ESI, XSRegisters.EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(XSRegisters.ESI, ObjectInfo.FieldDataOffset + 4);
            XS.Comment("Source index");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: SourceIndexDisplacement);
            XS.Add(XSRegisters.ESI, XSRegisters.EAX);

            XS.Comment("Destination array");
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(XSRegisters.EDI, ObjectInfo.FieldDataOffset + 4);
            XS.Comment("Destination index");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: DestinationIndexDisplacement);
            XS.Add(XSRegisters.EDI, XSRegisters.EAX);

            XS.Comment("Count");
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: CountDisplacement);
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
