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
        	new CPUx86.Cli();
        	new CPUx86.Move(Registers.DX, 0x60);
    /* Clear all keyboard buffers (output and command buffers) */
        	new CPUAll.Label(".clearBuffer");
			new CPUx86.InByte(Registers.AL, "064h");
        	new CPUx86.Move(Registers.AH, Registers.AL);
        	new CPUx86.Test(Registers.AH,1);
        	new CPUx86.JumpIfZero(".skipClearIO");
			new CPUx86.InByte(Registers.AL,Registers.DX);
        	new CPUAll.Label(".skipClearIO");
        	new CPUx86.Test(Registers.AH,2);
        	new JumpIfNotZero(".clearBuffer");
        	new Move(Registers.DX,0x64);
        	new Move(Registers.AL, 0xfe);
        	new Out(Registers.DX, Registers.AL);
            new CPUAll.Label(".loop");//failed... halt
            new CPUx86.Hlt();
            new CPUx86.Jump(".loop");
        }
    }
}
