using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "int")]
	public class Interrupt: Instruction {
		public readonly int Number;
		public const int INTO = 4;
		public Interrupt(int aNumber) {
			Number = aNumber;
		}

		public override string ToString() {
			if (Number == INTO) return "into";
			return "int " + Number;
		}
	}
}
