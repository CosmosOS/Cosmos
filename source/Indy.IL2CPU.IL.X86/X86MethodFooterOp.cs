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
		public readonly int LocalsCount = 0;
		public readonly bool HasReturnValue = false;
		public X86MethodFooterOp(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aMethodInfo.Arguments.Length > 0) {
				TotalArgsSize += aMethodInfo.Arguments[0].Offset + aMethodInfo.Arguments[0].Size;
			}
			if (aMethodInfo.Locals.Length > 0) {
				TotalLocalsSize += aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Offset + aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Size;
			}
			LocalsCount = aMethodInfo.Locals.Length;
			if (aMethodInfo.HasReturnValue) {
				TotalLocalsSize += 4;
				//LocalsCount++;   
				HasReturnValue = true;
			}
		}

		public override void DoAssemble() {
			// MtW: after trial and a huge amount of errors, this line doesn't seem to be needed
			//Assembler.Add(new CPU.Add("esp", TotalLocalsSize.ToString()));
			if (HasReturnValue) {
				//Assembler.Add(new Assembler.X86.Pop("eax"));
			}
			for (int i = 0; i < LocalsCount; i++) {
				if (HasReturnValue && i == 0) {
					continue;
				}
				Assembler.Add(new CPU.Add("esp", "4"));
			}
			Assembler.Add(new CPU.Pop("ebp"));
			Assembler.Add(new CPU.Ret(TotalArgsSize == 0 ? "" : TotalArgsSize.ToString()));
		}
	}
}