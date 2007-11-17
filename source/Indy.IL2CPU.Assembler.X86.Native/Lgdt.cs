using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.Native {
	[OpCode(0xFFFFFFFF, "lgdt")]
	public class Lgdt: X86.Instruction {
		public readonly string Operand;
		public Lgdt(string aOperand) {
			Operand = aOperand;
		}

		public override string ToString() {
			return "lgdt " + Operand;
		}
	}
}