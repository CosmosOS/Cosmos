using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JNE opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jne")]
	public class JumpIfNotEquals: Instruction {
		public readonly string Address;
		public JumpIfNotEquals(string aAddress) {
			Address = aAddress;
		}
		public override string ToString() {
			return "jne near " + Address;
		}
	}
}
