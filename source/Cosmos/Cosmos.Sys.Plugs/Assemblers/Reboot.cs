using Indy.IL2CPU.Plugs;
using Assembler=Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Sys.Plugs.Assemblers
{
    public class Reboot : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
        	new CPUx86.ClrInterruptFlag();
    /* Clear all keyboard buffers (output and command buffers) */
            new CPUAll.Label(".waitBuffer");
            new CPUx86.Move { SourceValue = 0x64, DestinationReg = Registers.DX };
            new CPUx86.In { DestinationReg=Registers.AL };
            new CPUx86.Test { DestinationReg = Registers.AL, SourceValue = 2 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = ".waitBuffer" };
            new CPUx86.Move { DestinationReg = Registers.AL, SourceValue = 0xD1 };
            new CPUx86.Move { DestinationReg = Registers.DX, SourceValue = 0x64 };
            new CPUx86.Out { DestinationReg= Registers.AL};
            new CPUAll.Label(".clearBuffer");
            new CPUx86.Move { SourceValue = 0x64, DestinationReg = Registers.DX };
            new CPUx86.In { DestinationReg = Registers.AL };
            new CPUx86.Test { DestinationReg = Registers.AL, SourceValue = 2 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = ".clearBuffer" };
            new CPUx86.Move { DestinationReg=Registers.AL, SourceValue=0xFE };
            new CPUx86.Move { DestinationReg = Registers.DX, SourceValue = 0x60 };
            new CPUx86.Out { DestinationReg= Registers.AL};
            new CPUAll.Label(".loop");//failed... halt
            new CPUx86.Halt();
            new CPUx86.Jump { DestinationLabel = ".loop" };
        }
    }
}
