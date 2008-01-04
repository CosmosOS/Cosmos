using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF,"jnc")]
	public class JumpNotCary: JumpBase {
		public JumpNotCary(string aTarget):base(aTarget) {
		}
		public override string ToString() {
			return "jnc " + Address;
		}
	}
}
