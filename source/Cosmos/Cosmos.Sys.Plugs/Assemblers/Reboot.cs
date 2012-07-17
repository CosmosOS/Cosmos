using Cosmos.IL2CPU.Plugs;
using Assembler=Cosmos.Assembler;
using CPUAll = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;

namespace Cosmos.Sys.Plugs.Assemblers
{
    public class Reboot : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
          new CPUx86.ClrInterruptFlag();
          /* Clear all keyboard buffers (output and command buffers) */
          new CPUAll.Label(".waitBuffer");
          new CPUx86.Mov {
            SourceValue = 0x64,
            DestinationReg = CPUx86.Registers.DX
          };
          new CPUx86.IN {
            DestinationReg = CPUx86.Registers.AL
          };
          new CPUx86.Test {
            DestinationReg = CPUx86.Registers.AL,
            SourceValue = 2
          };
          new CPUx86.ConditionalJump {
            Condition = CPUx86.ConditionalTestEnum.NotEqual,
            DestinationLabel = ".waitBuffer"
          };
          new CPUx86.Mov {
            DestinationReg = CPUx86.Registers.AL,
            SourceValue = 0xD1
          };
          new CPUx86.Mov {
            DestinationReg = CPUx86.Registers.DX,
            SourceValue = 0x64
          };
          new CPUx86.Out {
            DestinationReg = CPUx86.Registers.AL
          };
          new CPUAll.Label(".clearBuffer");
          new CPUx86.Mov {
            SourceValue = 0x64,
            DestinationReg = CPUx86.Registers.DX
          };
          new CPUx86.IN {
            DestinationReg = CPUx86.Registers.AL
          };
          new CPUx86.Test {
            DestinationReg = CPUx86.Registers.AL,
            SourceValue = 2
          };
          new CPUx86.ConditionalJump {
            Condition = CPUx86.ConditionalTestEnum.NotEqual,
            DestinationLabel = ".clearBuffer"
          };
          new CPUx86.Mov {
            DestinationReg = CPUx86.Registers.AL,
            SourceValue = 0xFE
          };
          new CPUx86.Mov {
            DestinationReg = CPUx86.Registers.DX,
            SourceValue = 0x60
          };
          new CPUx86.Out {
            DestinationReg = CPUx86.Registers.AL
          };
          new CPUAll.Label(".loop");//failed... halt
          new CPUx86.Halt();
          new CPUx86.Jump {
            DestinationLabel = ".loop"
          };
        }
    }
}
