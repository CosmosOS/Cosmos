using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class Pushd: Instruction {
		public readonly string[] Arguments;
		public Pushd(params string[] aArguments) {
			Arguments = aArguments;
		}

		public override string ToString() {
			string xResult = "pushd";
			foreach (string A in Arguments) {
				xResult += " " + A;
			}
			return xResult;
		}
	}
}
