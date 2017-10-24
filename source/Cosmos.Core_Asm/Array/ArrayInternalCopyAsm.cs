using XSharp.Assembler;
using Cosmos.IL2CPU.API;
using XSharp;
using CPUx86 = XSharp.Assembler.x86;

namespace Cosmos.Core_Asm
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

        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("Source");
            XS.Comment("Element size");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(XSRegisters.EAX, ObjectUtils.FieldDataOffset);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true); // element size
            XS.Comment("Source ptr");
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: SourceIndexDisplacement);
            XS.Multiply(XSRegisters.EBX);
            XS.Add(XSRegisters.EAX, ObjectUtils.FieldDataOffset + 4); // first element
            XS.Set(XSRegisters.ESI, XSRegisters.EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(XSRegisters.ESI, XSRegisters.EAX); // source ptr

            XS.Comment("Destination");
            XS.Comment("Element size");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(XSRegisters.EAX, ObjectUtils.FieldDataOffset);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true); // element size
            XS.Comment("Destination ptr");
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: DestinationIndexDisplacement);
            XS.Multiply(XSRegisters.ECX);
            XS.Add(XSRegisters.EAX, ObjectUtils.FieldDataOffset + 4); // first element
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(XSRegisters.EDI, XSRegisters.EAX); // destination ptr

            XS.Comment("Copy byte count");
            XS.Comment("Element size");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(XSRegisters.EAX, ObjectUtils.FieldDataOffset);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true); // element size
            XS.Comment("Count");
            XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: LengthDisplacement);
            XS.Multiply(XSRegisters.EDX);
            XS.Set(XSRegisters.ECX, XSRegisters.EAX);
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
