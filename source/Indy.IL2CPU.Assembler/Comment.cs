using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public class Comment: Instruction {
		public readonly string Text;

		public Comment(string aText) {
			Text = aText;
		}

		public override string ToString() {
			return Text;
		}
	}
}