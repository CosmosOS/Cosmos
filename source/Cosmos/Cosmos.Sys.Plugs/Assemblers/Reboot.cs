using Indy.IL2CPU.Plugs;
using Assembler=Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Sys.Plugs.Assemblers
{
    public class Reboot : AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler)
        {
            //TODO: Implement asm reboot, For now we just call Halt
            new CPUAll.Label(".loop");
            new CPUx86.Hlt();
            new CPUx86.Jump(".loop");
        }
    }
}
