using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    [Cosmos.Assembler.OpCode("ret")]
	public class Return: InstructionWithDestination {
        public Return() {
            DestinationValue = 0;
        }
    }
}