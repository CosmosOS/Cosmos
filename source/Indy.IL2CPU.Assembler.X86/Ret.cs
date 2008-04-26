using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "ret")]
	public class Ret: Instruction{
		public readonly int Argument;

        public Ret() : this(0) {
        }

        public Ret(int aArgument) {
            Argument = aArgument;
        }

        public override string ToString() {
            return ("ret " + ((Argument > 0) ? Argument.ToString() : "")).TrimEnd();
		}
	}
}