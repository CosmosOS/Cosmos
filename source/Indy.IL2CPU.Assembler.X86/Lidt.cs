using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "lidt")]
	public class Lidt: X86.Instruction {
		public readonly string Operand;
		public Lidt(string aOperand) {
			Operand = aOperand;
		}

		public override string ToString() {
			return "lidt " + Operand;
		}
	}
}