using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
	public class Pop: Instruction {
		public readonly string[] Arguments;

		public Pop(params string[] aArguments) {
			Arguments = aArguments;
		}

		public override string ToString() {
			string xResult = "pop";
			foreach (string A in Arguments) {
				xResult += " " + A;
			}
			return xResult;
		}
	}
}