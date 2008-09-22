using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "not")]
	public class Not: Instruction {
		private string mDestination;
		public Not(string aDestination) {
			mDestination = aDestination;
		}

		public override string ToString() {
			return "not " + mDestination;
		}
	}
}
