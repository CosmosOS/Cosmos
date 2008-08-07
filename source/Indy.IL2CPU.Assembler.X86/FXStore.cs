using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "fxrstore")]
    public class FXStore : Instruction
    {
		private string mDestination;
        public FXStore(string aDestination)
        {
			mDestination = aDestination;
		}
		
		public override string ToString() {
            return "fxrstor " + mDestination;
		}
	}
}
