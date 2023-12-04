using IL2CPU.API;
using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
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
            XS.Set(RAX, RBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(RAX, ObjectUtils.FieldDataOffset);
            XS.Set(RAX, RAX, sourceIsIndirect: true); // element size
            XS.Comment("Source ptr");
            XS.Set(RBX, RBP, sourceDisplacement: SourceIndexDisplacement);
            XS.Multiply(RBX);
            XS.Add(RAX, ObjectUtils.FieldDataOffset + 4); // first element
            XS.Set(RSI, RBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(RSI, RAX); // source ptr

            XS.Comment("Destination");
            XS.Comment("Element size");
            XS.Set(RAX, RBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(RAX, ObjectUtils.FieldDataOffset);
            XS.Set(RAX, RAX, sourceIsIndirect: true); // element size
            XS.Comment("Destination ptr");
            XS.Set(RCX, RBP, sourceDisplacement: DestinationIndexDisplacement);
            XS.Multiply(RCX);
            XS.Add(RAX, ObjectUtils.FieldDataOffset + 4); // first element
            XS.Set(RDI, RBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(RDI, RAX); // destination ptr

            XS.Compare(RDI, RSI);
            XS.Jump(ConditionalTestEnum.Equal, xArrayCopyEndLabel);

            XS.Comment("Copy byte count");
            XS.Comment("Element size");
            XS.Set(RAX, RBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(RAX, ObjectUtils.FieldDataOffset);
            XS.Set(RAX, RAX, sourceIsIndirect: true); // element size
            XS.Comment("Count");
            XS.Set(RDX, RBP, sourceDisplacement: LengthDisplacement);

            // if length is 0, jump to end
            XS.Compare(RDX, 0);
            XS.Jump(ConditionalTestEnum.Equal, xArrayCopyEndLabel);

            XS.Multiply(RDX);
            XS.Set(RCX, RAX);

            XS.Compare(RDI, RSI);
            XS.Jump(ConditionalTestEnum.GreaterThan, xArrayCopyReverseLabel);

            new Movs { Size = 8, Prefixes = InstructionPrefixes.Repeat };

            XS.Jump(xArrayCopyEndLabel);

            XS.Label(xArrayCopyReverseLabel);

            XS.Add(RSI, RCX);
            XS.Add(RDI, RCX);

            XS.Label(xArrayCopyReverseLoopLabel);

            XS.Decrement(RSI);
            XS.Decrement(RDI);
            XS.Decrement(RCX);

            XS.Set(AL, RSI, sourceIsIndirect: true);
            XS.Set(RDI, AL, destinationIsIndirect: true);

            XS.Compare(RCX, 0);
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
