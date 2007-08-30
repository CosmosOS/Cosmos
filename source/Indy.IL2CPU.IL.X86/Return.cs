using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
    class Return : Indy.IL2CPU.IL.Return {
        public override void Assemble(Instruction aInstruction, BinaryWriter aOutput) {
            Console.WriteLine("Ret");
        }
    }
}
