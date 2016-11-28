using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using ObjectInfo = Cosmos.IL2CPU.Plugs.System.ObjectImpl;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.Plugs.Assemblers.Array
{
    public class ArrayInternalCopyAsm : AssemblerMethod
    {
        private const int SourceArrayDisplacement = 36;
        private const int SourceIndexDisplacement = 32;
        private const int DestinationArrayDisplacement = 24;
        private const int DestinationIndexDisplacement = 16;
        private const int LengthDisplacement = 12;

        /* void Copy(
         *           Array sourceArray,			ebp + 36
         *			 int sourceIndex,			ebp + 28
         *			 Array destinationArray,	ebp + 24
         *			 int destinationIndex,		ebp + 16
         *			 int length,				ebp + 12
         *			 bool reliable);			ebp + 8
         */

        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("Source");
            XS.Comment("Element size");
            XS.Set(EAX, EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(EAX, ObjectInfo.FieldDataOffset);
            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
            XS.Comment("Source ptr");
            XS.Set(EBX, EBP, sourceDisplacement: SourceIndexDisplacement);
            XS.Multiply(EBX);
            XS.Add(EAX, ObjectInfo.FieldDataOffset + 4); // first element
            XS.Set(ESI, EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(ESI, EAX); // source ptr

            XS.Comment("Destination");
            XS.Comment("Element size");
            XS.Set(EAX, EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(EAX, ObjectInfo.FieldDataOffset);
            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
            XS.Comment("Destination ptr");
            XS.Set(ECX, EBP, sourceDisplacement: DestinationIndexDisplacement);
            XS.Multiply(ECX);
            XS.Add(EAX, ObjectInfo.FieldDataOffset + 4); // first element
            XS.Set(EDI, EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(EDI, EAX); // destination ptr

            XS.Comment("Copy byte count");
            XS.Comment("Element size");
            XS.Set(EAX, EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(EAX, ObjectInfo.FieldDataOffset);
            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
            XS.Comment("Count");
            XS.Set(EDX, EBP, sourceDisplacement: LengthDisplacement);
            XS.Multiply(EDX);
            XS.Set(ECX, EAX);
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
