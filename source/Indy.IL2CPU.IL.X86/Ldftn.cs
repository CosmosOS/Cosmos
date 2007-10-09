using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldftn)]
	public class Ldftn: Op {
		private string mFunctionLabel;

		public Ldftn(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			MethodReference xMethodRef = aInstruction.Operand as MethodReference;
			if (xMethodRef == null) {
				throw new Exception("Unable to determine Method!");
			}
			mFunctionLabel = new CPU.Label(xMethodRef).Name;
		}

		public override void DoAssemble() {
			Pushd(mFunctionLabel);
			Assembler.StackSizes.Push(4);
		}
	}
}