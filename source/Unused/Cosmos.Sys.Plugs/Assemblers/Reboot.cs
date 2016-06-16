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
          XS.Mov(XSRegisters.DX, 0x64);
          XS.InFromDX(XSRegisters.AL);
          XS.Test(XSRegisters.AL, 2);
          new CPUx86.ConditionalJump {
            Condition = CPUx86.ConditionalTestEnum.NotEqual,
            DestinationLabel = ".waitBuffer"
          };
          XS.Mov(XSRegisters.AL, 0xD1);
          XS.Mov(XSRegisters.DX, 0x64);
          XS.OutToDX(XSRegisters.AL);
          new CPUAll.Label(".clearBuffer");
          XS.Mov(XSRegisters.DX, 0x64);
          XS.InFromDX(XSRegisters.AL);
          XS.Test(XSRegisters.AL, 2);
          new CPUx86.ConditionalJump {
            Condition = CPUx86.ConditionalTestEnum.NotEqual,
            DestinationLabel = ".clearBuffer"
          };
          XS.Mov(XSRegisters.AL, 0xFE);
          XS.Mov(XSRegisters.DX, 0x60);
          XS.OutToDX(XSRegisters.AL);
          new CPUAll.Label(".loop");//failed... halt
          new CPUx86.Halt();
          XS.Jump(".loop");
        }
    }
}
