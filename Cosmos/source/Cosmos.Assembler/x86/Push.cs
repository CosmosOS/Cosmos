using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    [Cosmos.Assembler.OpCode("push")]
    public class Push : InstructionWithDestinationAndSize {

        public Push():base("push") {
            Size = 32;
        }
    }
}
