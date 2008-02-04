using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Indy.IL2CPU.IL.X86.Win32 {
	public class Call: X86.Call {
		public Call(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		protected override void HandleDebuggerBreak() {
			new Assembler.X86.Interrupt(3);
		}
	}
}
