using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[Obsolete("This instruction is a copy of JumpIfGreaterOrEqual, and has wrong opcode in attribute")]
	[OpCode(0xFFFFFFFF, "jbe")]
	public class JumpIfGreaterOrEquals: JumpBase {
		public JumpIfGreaterOrEquals(string aAddress)
			: base(aAddress) {
		}

		public override string ToString() {
			return "jge " + Address;
		}
	}
}