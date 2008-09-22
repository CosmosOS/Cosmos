using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.SSE {
	[OpCode(0xFFFFFFFF, "addss")]
	public class AddSS: Instruction {
		private string mDestination;
		private string mSource;
        public AddSS(string aDestination, string aSource)
        {
			mDestination = aDestination;
			mSource = aSource;
		}
		
		public override string ToString() {
			return "addss " + mDestination + ", " + mSource;
		}
	}
}
