using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
    class Return : Indy.IL2CPU.IL.Return {
        public override void Process(Instruction aInstruction) {
            Console.WriteLine("Ret");
        }
    }
}
