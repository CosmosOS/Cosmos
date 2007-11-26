using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "int")]
	public class Interrupt: Instruction {
		public readonly int Number;
		public Interrupt(int aNumber) {
			Number = aNumber;
		}

		public override string ToString() {
			return "int " + Number;
		}
	}
}
