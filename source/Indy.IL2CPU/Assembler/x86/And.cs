using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "and")]
	public class And: Instruction {
		private string mDestination;
		private string mSource;
		public And(string aDestination, string aSource) {
			mDestination = aDestination;
			mSource = aSource;
		}
		
		public override string ToString() {
			return "and " + mDestination + ", " + mSource;
		}
	}
}
