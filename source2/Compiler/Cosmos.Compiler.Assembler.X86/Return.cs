using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("ret")]
	public class Return: InstructionWithDestination {
        public Return() {
            DestinationValue = 0;
        }
    }
}