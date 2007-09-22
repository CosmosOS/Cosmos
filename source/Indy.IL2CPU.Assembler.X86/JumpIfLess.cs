using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JL opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jl")]
	public class JumpIfLess: Instruction {
		public readonly string Address;
		public JumpIfLess(string aAddress) {
			Address = aAddress;
		}
		public override string ToString() {
			return "jl " + Address;
		}
	}
}