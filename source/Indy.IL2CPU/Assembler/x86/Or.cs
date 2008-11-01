using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "or")]
	public class Or: Instruction {
		private string mDestination;
		private string mSource;
        public Or(string aDestination, int aValue):this(aDestination, aValue.ToString()) {
        }
		public Or(string aDestination, string aSource) {
			mDestination = aDestination;
			mSource = aSource;
		}
		
		public override string ToString() {
			return "or " + mDestination + ", " + mSource;
		}
	}
}
