/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.NativeX86 {
	public class NativeX86MethodFooterOp: X86MethodFooterOp {
		private bool mIsInterruptHandler = false;
		public NativeX86MethodFooterOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			foreach (CustomAttribute xAttrib in aMethodInfo.MethodDefinition.CustomAttributes) {
				if(xAttrib.Constructor.DeclaringType.FullName == typeof(InterruptServiceRoutineAttribute).FullName) {
					mIsInterruptHandler = true;
					break;
				}
			}
		}

		public override void DoAssemble() {
			// MtW: after trial and a huge amount of errors, this line doesn't seem to be needed
			//Assembler.Add(new CPU.Add("esp", TotalLocalsSize.ToString()));
			AssembleFooter(HasReturnValue, Assembler, LocalsCount, TotalArgsSize);
		}

		public static void AssembleFooter(bool aHasReturnValue, Assembler.Assembler aAssembler, int aLocalsCount, int aTotalArgsSize) {
			if (aHasReturnValue) {
				aAssembler.Add(new Assembler.X86.Pop("eax"));
			}
			for (int i = 0; i < aLocalsCount; i++) {
				aAssembler.Add(new CPU.Add("esp", "4"));
			}
			aAssembler.Add(new CPU.Pop("ebp"));
			aAssembler.Add(new CPU.Ret(aTotalArgsSize == 0 ? "" : aTotalArgsSize.ToString()));
		}
	}
}*/