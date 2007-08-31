using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class Xor: Instruction {
		public readonly string Arg1;
		public readonly string Arg2;

		public Xor(string aArg1, string aArg2) {
			Arg1 = aArg1;
			Arg2 = aArg2;
		}

		public override string ToString() {
			return "xor " + Arg1 + "," + Arg2;
		}
	}
}
