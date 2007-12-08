using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JE opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "je")]
	public class JumpIfEquals: JumpBase {
		public JumpIfEquals(string aAddress):base(aAddress) {
		}
		public override string ToString() {
			return "je near " + Address;
		}
	}
}
