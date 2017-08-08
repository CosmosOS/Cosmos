using Cosmos.IL2CPU.Plugs;
using Assembler=Cosmos.Assembler;
using CPUAll = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;

namespace Cosmos.Sys.Plugs.Assemblers{
    public class ShutDown : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
          //ACPI Way...ONLY QEMu And Boschs
          XS.Mov(XSRegisters.DX, 0xB004);
          new CPUx86.Mov {
            DestinationReg = CPUx86.Registers.AX,
            SourceValue = 0x0 | 0x2000
          };
          new CPUx86.Out {
            DestinationReg = CPUx86.Registers.AX,
            Size = (byte)CPUx86.Instruction.InstructionSize.Word
          };
          //TODO: ACPI Way...see http://forum.osdev.org/viewtopic.php?t=16990
        }
    }
}
