using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [Cosmos.Assembler.OpCode("push")]
    public class Push : InstructionWithDestinationAndSize {

        public Push() {
            Size = 32;
        }
    }
}
