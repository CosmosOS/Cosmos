using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class Array_InternalCopy : AssemblerMethod
    {
        private const int SourceArrayDisplacement = 0x1C;
        private const int SourceIndexDisplacement = 0x18;
        private const int DestinationArrayDisplacement = 0x14;
        private const int DestinationIndexDisplacement = 0x10;
        private const int LengthDisplacement = 0xC;

        /* void Copy(Array sourceArray,			ebp + 0x1C
         *			 int sourceIndex,			ebp + 0x18
         *			 Array destinationArray,	ebp + 0x14
         *			 int destinationIndex,		ebp + 0x10
         *			 int length,				ebp + 0xC
         *			 bool reliable);			ebp + 0x8
         */
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = SourceArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EAX, SourceIsIndirect = true }; // dereference memory handle to pointer
            XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceValue = 12, Size = 32 }; // pointer is at the element size
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EAX, SourceIsIndirect = true }; // element size
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EBX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = SourceIndexDisplacement };
            XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 16);
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = SourceArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.ESI, SourceIsIndirect = true }; // dereference memory handle to pointer
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX)); // source ptr
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDX, SourceReg = CPUx86.RegistersEnum.EDX, SourceIsIndirect = true }; // dereference memory handle to pointer
            XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
            new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceValue = 12, Size = 32 }; // pointer is at element size
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EAX, SourceIsIndirect = true }; // dereference handle to pointer
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationIndexDisplacement };
            XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 16);
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EDI, SourceIsIndirect = true }; // dereference handle to pointer
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

            // calculate byte count to copy
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EAX, SourceIsIndirect = true }; // dereference memory handle to pointer
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 12);
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EAX, SourceIsIndirect = true };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = LengthDisplacement };
            XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
