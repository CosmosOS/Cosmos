using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "lgdt")]
	public class Loadgdt: X86.Instruction {
		public readonly string Operand;
		public Loadgdt(string aOperand) {
			Operand = aOperand;
		}

		public override string ToString() {
			return "lgdt " + Operand;
		}
	}
}