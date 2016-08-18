using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using ObjectInfo = Cosmos.IL2CPU.Plugs.System.ObjectImpl;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class Array_InternalCopy : AssemblerMethod
    {
        private const int SourceArrayDisplacement = 0x24;
        private const int SourceIndexDisplacement = 0x1C;
        private const int DestinationArrayDisplacement = 0x18;
        private const int DestinationIndexDisplacement = 0x10;
        private const int LengthDisplacement = 0xC;

        /* void Copy(Array sourceArray,			ebp + 0x24
         *			 int sourceIndex,			ebp + 0x1C
         *			 Array destinationArray,	ebp + 0x18
         *			 int destinationIndex,		ebp + 0x10
         *			 int length,				ebp + 0xC
         *			 bool reliable);			ebp + 0x8
         */
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Comment("Source");
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Push(XSRegisters.EAX);
            new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceValue = ObjectInfo.FieldDataOffset, Size = 32 }; // pointer is at the element size
            XS.Pop(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true); // element size
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: SourceIndexDisplacement);
            XS.Multiply(XSRegisters.EBX);
            XS.Add(XSRegisters.EAX, 16);
            XS.Set(XSRegisters.ESI, XSRegisters.EBP, sourceDisplacement: SourceArrayDisplacement);
            XS.Add(XSRegisters.ESI, XSRegisters.EAX); // source ptr

            XS.Comment("Destination");
            XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Push(XSRegisters.EDX);
            new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceValue = ObjectInfo.FieldDataOffset, Size = 32 }; // pointer is at element size
            XS.Pop(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true); // element size
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: DestinationIndexDisplacement);
            XS.Multiply(XSRegisters.ECX);
            XS.Add(XSRegisters.EAX, 16);
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(XSRegisters.EDI, XSRegisters.EAX); // destination ptr

            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: DestinationArrayDisplacement);
            XS.Add(XSRegisters.EAX, 12);
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
            XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: LengthDisplacement);
            XS.Multiply(XSRegisters.EDX);
            XS.Set(XSRegisters.ECX, XSRegisters.EAX);
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
