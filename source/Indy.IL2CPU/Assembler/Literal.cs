using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	[Obsolete("Try using dedicated opcodes")]
	public class Literal: Instruction {
		public readonly string Data;

		public Literal(string aData) {
			Data = aData;
		}

		public override string ToString() {
			return Data;
		}
	}
}