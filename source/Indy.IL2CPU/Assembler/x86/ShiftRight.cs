using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "shr")]
	public class ShiftRight: Instruction {
		private string mDestination;
		private string mCount;
		public ShiftRight(string aDestination, string aCount) {
			mDestination = aDestination;
			mCount = aCount;
		}
		public override string ToString() {
			return "shr " + mDestination + ", " + mCount;
		}
	}
}