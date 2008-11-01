using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    [OpCode(0xFFFFFFFF, "%endif")]
    public class EndIfDefined : Instruction {
        public EndIfDefined() {
        }

        public override string ToString() {
            return "%endif";
        }
    }
}