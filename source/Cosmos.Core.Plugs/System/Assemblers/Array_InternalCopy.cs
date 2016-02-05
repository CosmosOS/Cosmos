using Cosmos.IL2CPU.Plugs;

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
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = SourceArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true }; // dereference memory handle to pointer
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceValue = 12, Size = 32 }; // pointer is at the element size
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true }; // element size
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = SourceIndexDisplacement };
            new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EBX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 16 };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = SourceArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESI, SourceIsIndirect = true }; // dereference memory handle to pointer
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EAX }; // source ptr
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX, SourceIsIndirect = true }; // dereference memory handle to pointer
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceValue = 12, Size = 32 }; // pointer is at element size
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true }; // dereference handle to pointer
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationIndexDisplacement };
            new CPUx86.Multiply { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 16 };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI, SourceIsIndirect = true }; // dereference handle to pointer
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EAX };

            // calculate byte count to copy
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = DestinationArrayDisplacement };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true }; // dereference memory handle to pointer
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 12 };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = LengthDisplacement };
            new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EAX };
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
