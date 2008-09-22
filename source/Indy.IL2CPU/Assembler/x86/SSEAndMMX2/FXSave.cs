using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "fxsave")]
	public class FXSave : Instruction {
		private string mDestination;
        public FXSave(string aDestination)
        {
			mDestination = aDestination;
		}
		
		public override string ToString() {
			return "fxsave " + mDestination;
		}
	}
}
