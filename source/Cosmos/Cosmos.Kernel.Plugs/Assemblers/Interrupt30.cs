using System;
using System.Linq;
using Cosmos.Compiler.Assembler;
using Cosmos.IL2CPU.Plugs;
using CPU = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public class Interrupt30 : AssemblerMethod {
        /*
         * Arguments with their addresses:
         *   ref uint aEAX  : ebp - 20
         *   ref uint aEBX  : ebp - 16
         *   ref uint aECX  : ebp - 12
         *   ref uint aEDX  : ebp - 8
         */

        public override void AssembleNew(object aAssembler, object aMethodInfo) {
          
        }
    }
}