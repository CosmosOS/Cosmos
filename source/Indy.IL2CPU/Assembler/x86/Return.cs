using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "ret")]
	public class Return: New_Instruction{
		public readonly int Argument;

        public Return() : this(0) {
        }

        public Return(int aArgument) {
            Argument = aArgument;
        }

        public override string ToString() {
            return ("ret " + ((Argument > 0) ? Argument.ToString() : "")).TrimEnd();
		}
	}
}