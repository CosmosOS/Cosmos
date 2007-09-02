using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JB opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jb")]
	public class JumpIfGreater: Instruction {
		public readonly string Address;
		public JumpIfGreater(string aAddress) {
			Address = aAddress;
		}
		public override string ToString() {
			return "jb " + Address;
		}
	}
}
