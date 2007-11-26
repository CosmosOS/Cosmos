using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "jbe")]
	public class JumpIfGreaterOrEquals: Instruction {
		public readonly string Address;
		public JumpIfGreaterOrEquals(string aAddress) {
			Address = aAddress;
		}

		public override string ToString() {
			return "jge " + Address;
		}
	}
}