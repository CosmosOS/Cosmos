using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ret)]
	public class Ret: Op {
		private bool mHasReturn;
		public Ret(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			mHasReturn = aMethodInfo.HasReturnValue;
		}
		public override void DoAssemble() {
			if (mHasReturn) {
				//Push("eax");
			}
		}
	}
}