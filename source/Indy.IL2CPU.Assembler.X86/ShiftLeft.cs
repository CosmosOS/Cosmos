using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "shl")]
	public class ShiftLeft: Instruction {
		private string mDestination;
		private string mSource;
		private string mCount;
		public ShiftLeft(string aDestination, string aSource, string aCount) {
			mDestination = aDestination;
			mSource = aSource;
			mCount = aCount;
		}
		public override string ToString() {
			return "shld " + mDestination + ", " + mSource + ", " + mCount;
		}
	}
}