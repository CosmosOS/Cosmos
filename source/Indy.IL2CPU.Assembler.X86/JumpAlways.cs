using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JMP opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jmp")]
	public class JumpAlways: Instruction {
		public readonly string Address;
		public JumpAlways(string aAddress) {
			Address = aAddress;
		}
		public override string ToString() {
			return "jmp " + Address;
		}
	}
}
