using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "test")]
	public class Test: Instruction {
		public readonly string Arg1;
		public readonly UInt32 Arg2;

		public Test(string aArg1, UInt32 aArg2) {
			Arg1 = aArg1;
			Arg2 = aArg2;
		}

		public override string ToString() {
			return "test " + Arg1 + ", " + Arg2;
		}
	}
}