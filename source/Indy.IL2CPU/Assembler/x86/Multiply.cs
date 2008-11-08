using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("mul")]
	public class Multiply: InstructionWithDestinationAndSize {
		public readonly string Address1;
		public Multiply(string aAddress1) {
			Address1 = aAddress1;
		}
		public override string ToString() {
			return "mul " + Address1;
		}
	}
}
