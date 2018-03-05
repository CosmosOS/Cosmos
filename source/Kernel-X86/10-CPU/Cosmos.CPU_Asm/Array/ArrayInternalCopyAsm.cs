using IL2CPU.API;
using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;
using static XSharp.XSRegisters;

namespace Cosmos.CPU_Asm
{
    public class ArrayInternalCopyAsm : AssemblerMethod
    {
        private const int SourceArrayDisplacement = 36;
        private const int SourceIndexDisplacement = 28;
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
            var xArrayCopyReverseLabel = "ArrayCopy_Reverse";
            var xArrayCopyReverseLoopLabel = "ArrayCopy_Reverse_Loop";
            var xArrayCopyEndLabel = "ArrayCopy_End";

            XS.Comment("Source");
            XS.Comment("Element size");
            XS.Set(EAX, EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(EAX, ObjectUtils.FieldDataOffset);
            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
            XS.Comment("Source ptr");
            XS.Set(EBX, EBP, sourceDisplacement: SourceIndexDisplacement);
            XS.Multiply(EBX);
            XS.Add(EAX, ObjectUtils.FieldDataOffset + 4); // first element
            XS.Set(ESI, EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(ESI, EAX); // source ptr

            XS.Comment("Destination");
            XS.Comment("Element size");
            XS.Set(EAX, EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(EAX, ObjectUtils.FieldDataOffset);
            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
            XS.Comment("Destination ptr");
            XS.Set(ECX, EBP, sourceDisplacement: DestinationIndexDisplacement);
            XS.Multiply(ECX);
            XS.Add(EAX, ObjectUtils.FieldDataOffset + 4); // first element
            XS.Set(EDI, EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(EDI, EAX); // destination ptr

            XS.Compare(EDI, ESI);
            XS.Jump(ConditionalTestEnum.Equal, xArrayCopyEndLabel);

            XS.Comment("Copy byte count");
            XS.Comment("Element size");
            XS.Set(EAX, EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(EAX, ObjectUtils.FieldDataOffset);
            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
            XS.Comment("Count");
            XS.Set(EDX, EBP, sourceDisplacement: LengthDisplacement);

            // if length is 0, jump to end
            XS.Compare(EDX, 0);
            XS.Jump(ConditionalTestEnum.Equal, xArrayCopyEndLabel);

            XS.Multiply(EDX);
            XS.Set(ECX, EAX);

            XS.Compare(EDI, ESI);
            XS.Jump(ConditionalTestEnum.GreaterThan, xArrayCopyReverseLabel);

            new Movs { Size = 8, Prefixes = InstructionPrefixes.Repeat };

            XS.Jump(xArrayCopyEndLabel);

            XS.Label(xArrayCopyReverseLabel);

            XS.Add(ESI, ECX);
            XS.Add(EDI, ECX);

            XS.Label(xArrayCopyReverseLoopLabel);

            XS.Decrement(ESI);
            XS.Decrement(EDI);
            XS.Decrement(ECX);

            XS.Set(AL, ESI, sourceIsIndirect: true);
            XS.Set(EDI, AL, destinationIsIndirect: true);

            XS.Compare(ECX, 0);
            XS.Jump(ConditionalTestEnum.NotEqual, xArrayCopyReverseLoopLabel);

            XS.Label(xArrayCopyEndLabel);
        }
    }
}

// Old implementation
// (it's a good memcpy implementation, as it doesn't check for array overlapping, so it can't be used for Array.Copy)

//using Cosmos.Assembler;
//using Cosmos.Assembler.x86;
//using IL2CPU.API;
//using XSharp.Common;
//using static XSharp.Common.XSRegisters;

//namespace Cosmos.Core_Asm
//{
//    public class ArrayInternalCopyAsm : AssemblerMethod
//    {
//        private const int SourceArrayDisplacement = 36;
//        private const int SourceIndexDisplacement = 28;
//        private const int DestinationArrayDisplacement = 24;
//        private const int DestinationIndexDisplacement = 16;
//        private const int LengthDisplacement = 12;

//        /* void Copy(
//         *           Array sourceArray,			ebp + 36
//         *			 int sourceIndex,			ebp + 28
//         *			 Array destinationArray,	ebp + 24
//         *			 int destinationIndex,		ebp + 16
//         *			 int length,				ebp + 12
//         *			 bool reliable);			ebp + 8
//         */

//        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
//        {
//            XS.Comment("Source");
//            XS.Comment("Element size");
//            XS.Set(EAX, EBP, sourceDisplacement: SourceArrayDisplacement);
//            XS.Add(EAX, ObjectUtils.FieldDataOffset);
//            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
//            XS.Comment("Source ptr");
//            XS.Set(EBX, EBP, sourceDisplacement: SourceIndexDisplacement);
//            XS.Multiply(EBX);
//            XS.Add(EAX, ObjectUtils.FieldDataOffset + 4); // first element
//            XS.Set(ESI, EBP, sourceDisplacement: SourceArrayDisplacement);
//            XS.Add(ESI, EAX); // source ptr

//            XS.Comment("Destination");
//            XS.Comment("Element size");
//            XS.Set(EAX, EBP, sourceDisplacement: DestinationArrayDisplacement);
//            XS.Add(EAX, ObjectUtils.FieldDataOffset);
//            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
//            XS.Comment("Destination ptr");
//            XS.Set(ECX, EBP, sourceDisplacement: DestinationIndexDisplacement);
//            XS.Multiply(ECX);
//            XS.Add(EAX, ObjectUtils.FieldDataOffset + 4); // first element
//            XS.Set(EDI, EBP, sourceDisplacement: DestinationArrayDisplacement);
//            XS.Add(EDI, EAX); // destination ptr

//            XS.Comment("Copy byte count");
//            XS.Comment("Element size");
//            XS.Set(EAX, EBP, sourceDisplacement: DestinationArrayDisplacement);
//            XS.Add(EAX, ObjectUtils.FieldDataOffset);
//            XS.Set(EAX, EAX, sourceIsIndirect: true); // element size
//            XS.Comment("Count");
//            XS.Set(EDX, EBP, sourceDisplacement: LengthDisplacement);
//            XS.Multiply(EDX);
//            XS.Set(ECX, EAX);
//            new Movs { Size = 8, Prefixes = InstructionPrefixes.Repeat };
//        }
//    }
//}
