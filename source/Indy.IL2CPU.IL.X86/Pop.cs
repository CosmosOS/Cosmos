using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
    class Pop : Indy.IL2CPU.IL.Pop {
        public override void Assemble(Instruction aInstruction) {
            Console.WriteLine("Pop");
        }
    }
}
