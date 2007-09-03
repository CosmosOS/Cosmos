using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodFooterOp: MethodFooterOp {
		public readonly int TotalLocalsSize = 0;
		public readonly int TotalArgsSize = 0;
		public X86MethodFooterOp(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aMethodInfo.Arguments.Length > 0) {
				TotalArgsSize += aMethodInfo.Arguments[aMethodInfo.Arguments.Length - 1].Offset + aMethodInfo.Arguments[aMethodInfo.Arguments.Length - 1].Size;
			}
			if (aMethodInfo.Locals.Length > 0) {
				TotalLocalsSize += aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Offset + aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Size;
			}
		}

		public override void Assemble() {
			Assembler.Add(new CPU.Add("esp", TotalLocalsSize.ToString()));
			Assembler.Add(new CPU.Ret(TotalArgsSize.ToString()));
		}
	}
}