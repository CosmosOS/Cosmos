using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Plugs;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public class Interrupt30 : AssemblerMethod {
        /*
         * Arguments with their addresses:
         *   ref uint aEAX  : ebp - 20
         *   ref uint aEBX  : ebp - 16
         *   ref uint aECX  : ebp - 12
         *   ref uint aEDX  : ebp - 8
         */

        public override void Assemble(Assembler aAssembler) {
            
        }
    }
}