using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
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