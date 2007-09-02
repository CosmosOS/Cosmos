using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "call")]
	public class Call: Instruction {
		public readonly string Address;
		
     public Call(string aAddress) {
	  	Address = aAddress;
		}

		public override string ToString() {
			return "call " + Address;
		}
	}
}