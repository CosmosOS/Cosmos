using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	/// <summary>
	/// Represents the JZ opcode
	/// </summary>
	public class JumpIfZero: Instruction {
		public readonly string Address;
		public JumpIfZero(string aAddress) {
			Address = aAddress;
		}
		public override string ToString() {
			return "jz " + Address;
		}
	}
}