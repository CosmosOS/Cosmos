using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
    class Pop : Indy.IL2CPU.IL.Pop {
        public override void Process(Instruction aInstruction) {
            Console.WriteLine("Pop");
        }
    }
}
