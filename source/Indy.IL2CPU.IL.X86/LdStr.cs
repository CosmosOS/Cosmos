using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
    class LdStr : Indy.IL2CPU.IL.LdStr {
        public override void Assemble(Instruction aInstruction) {
            // Operand contains the string to be loaded
            Console.WriteLine("LdStr, string = '{0}'", aInstruction.Operand);
        }
    }
}
