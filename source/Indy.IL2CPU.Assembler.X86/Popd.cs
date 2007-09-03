using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "popd")]
	public class Popd: Instruction {
		public readonly string[] Arguments;

		public Popd(params string[] aArguments) {
			Arguments = aArguments;
		}

		public override string ToString() {
			string xResult = "popd";
			foreach (string A in Arguments) {
				xResult += " " + A;
			}
			return xResult;
		}
	}
}
