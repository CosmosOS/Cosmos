using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Indy.IL2CPU.IL.X86.Native.CustomImplementations.System.Diagnostics;

namespace Indy.IL2CPU.IL.X86.Native {
	public class Call: X86.Call {
		public Call(Instruction aInstruction, MethodInformation aMethodInfo): base(aInstruction, aMethodInfo) {
		}

		protected override void HandleDebuggerBreak() {
			var xMethod = Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.Diagnostics.Debugger"), "Break", new string[0]);
			Engine.QueueMethod(xMethod);
			//new Assembler.X86.Call(Indy.IL2CPU.Assembler.Label.GenerateLabelName(xMethod));
			new Assembler.X86.Call(DebuggerImpl.BreakMethodName);

			//new IL2CPU.Assembler.X86.Native.Break();
		}
	}
}
