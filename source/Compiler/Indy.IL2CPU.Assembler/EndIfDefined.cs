using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    [OpCode("%endif")]
    public class EndIfDefined : Instruction, IEndIfDefined {
        public override string ToString() {
            return this.GetAsText();
        }
    }
}