using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class Buffer_BlockCopy : AssemblerMethod
    {

        /*public static void BlockCopy(
         *			Array src, [ebp + 24]
         *			int srcOffset, [ebp + 20]
         *			Array dst, [ebp + 16]
         *			int dstOffset, [ebp + 12]
         *			int count); [ebp + 8]
         */
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 24 };
            new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESI, SourceValue = 16 };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 20 };
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 16 };
            new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.EDI, SourceValue = 16 };
            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 12 };
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
