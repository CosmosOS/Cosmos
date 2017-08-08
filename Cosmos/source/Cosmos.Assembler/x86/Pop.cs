using System;
using System.Linq;

namespace Cosmos.Assembler.x86 {
    [Cosmos.Assembler.OpCode("pop")]
	public class Pop: InstructionWithDestinationAndSize{
        public Pop() : base("pop")
        {
        }
	}

}