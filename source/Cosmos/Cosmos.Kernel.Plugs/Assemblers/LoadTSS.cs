using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Plugs;
using CPU=Indy.IL2CPU.Assembler;
using CPUx86=Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class LoadTSS: AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler) {
            new CPUx86.Move("ax",
                            "0x18");
            new CPUx86.LoadTR("ax");
        }
    }
}