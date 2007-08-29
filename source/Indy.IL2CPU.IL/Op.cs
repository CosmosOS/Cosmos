using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class Op {
		public abstract void Process(Instruction aInstruction);
	}
}
