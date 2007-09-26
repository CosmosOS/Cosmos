using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "neg")]
	public class Neg: Instruction {
		private string mDestination;
		public Neg(string aDestination) {
			mDestination = aDestination;
		}
		public override string ToString() {
			return "neg " + mDestination;
		}
	}
}