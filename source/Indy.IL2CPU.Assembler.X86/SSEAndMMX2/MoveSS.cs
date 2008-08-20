using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.SSE {
	[OpCode(0xFFFFFFFF, "movss")]
    public class MoveSS : Instruction
    {
		private string mDestination;
		private string mSource;
		public MoveSS(string aDestination, string aSource) {
			mDestination = aDestination;
			mSource = aSource;
		}
		
		public override string ToString() {
			return "movss " + mDestination + ", " + mSource;
		}
	}
}
