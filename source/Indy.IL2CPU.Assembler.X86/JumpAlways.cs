using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JMP opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jmp")]
	public class Jump: JumpBase {
		public Jump(string aAddress)
			: base(aAddress) {
		}
		public override string ToString() {
			return "jmp " + Address;
		}
	}
}
