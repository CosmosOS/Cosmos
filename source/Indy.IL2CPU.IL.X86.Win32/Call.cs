using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public class Call: X86.Call {
		public Call(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		protected override void HandleDebuggerBreak() {
			new Assembler.X86.Interrupt(3);
		}
	}
}
