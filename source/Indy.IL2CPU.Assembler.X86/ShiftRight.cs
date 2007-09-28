using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "shr")]
	public class ShiftRight: Instruction {
		private string mDestination;
		private string mCount;
		private string mSource;
		public ShiftRight(string aDestination, string aSource, string aCount) {
			mDestination = aDestination;
			mCount = aCount;
			mSource = aSource;
		}
		public override string ToString() {
			return "shrd " + mDestination + ", " + mSource + ", " + mCount;
		}
	}
}