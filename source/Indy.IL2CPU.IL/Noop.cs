using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Nop)]
	public class Noop: Op {
        public override void Assemble(Instruction aInstruction) {
			Console.WriteLine("NoOp encountered");
		}
	}
}
