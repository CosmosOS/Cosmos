using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public class Push: Instruction {
		public readonly string[] Arguments;
		public Push(params string[] aArguments) {
			Arguments = aArguments;
		}

		public override string ToString() {
			string xResult = "push";
			foreach (string A in Arguments) {
				xResult += " " + A;
			}
			return xResult;
		} 
	}
}