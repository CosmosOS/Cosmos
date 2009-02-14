using Indy.IL2CPU.Plugs;
using Assembler=Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86;
namespace Cosmos.Sys.Plugs.Assemblers{
    public class ShutDown : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
            //ACPI Way...ONLY QEMu And Boschs
            new CPUx86.Move { DestinationReg = CPUx86.Registers.DX, SourceValue = 0xB004 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceValue= 0x0 | 0x2000};
            new CPUx86.Out { DestinationReg = CPUx86.Registers.AX, Size = (byte)CPUx86.Instruction.InstructionSize.Word };
            //TODO: ACPI Way...see http://forum.osdev.org/viewtopic.php?t=16990
            
        }
    }
}
