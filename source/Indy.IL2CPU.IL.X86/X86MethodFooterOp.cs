using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodFooterOp: MethodFooterOp {
		public override void Assemble(Instruction aInstruction, MethodInformation aMethodInfo) {
			int xTotalArgSize = 0;
			if (aMethodInfo.Arguments.Length > 0) {
				xTotalArgSize += aMethodInfo.Arguments[aMethodInfo.Arguments.Length - 1].Offset + aMethodInfo.Arguments[aMethodInfo.Arguments.Length - 1].Size;
			}
			int xTotalLocalsSize = 0;
			if (aMethodInfo.Locals.Length > 0) {
				xTotalLocalsSize += aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Offset + aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Size;
			}
			Assembler.Add(new CPU.Add("esp", xTotalLocalsSize.ToString()));
			Assembler.Add(new CPU.Ret(xTotalArgSize.ToString()));
		}
	}
}