using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JE opcode
	/// </summary>
	public class JumpIfEquals: Instruction {
		public readonly string Address;
		public JumpIfEquals(string aAddress) {
			Address = aAddress;
		}
		public override string ToString() {
			return "je " + Address;
		}
	}
}
