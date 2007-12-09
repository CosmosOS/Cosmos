using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

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
			mFunctionLabel = CPU.Label.GenerateLabelName(xMethodRef);
			Engine.QueueMethodRef(xMethodRef);
		}

		public override void DoAssemble() {
			new CPUx86.Pushd(mFunctionLabel);
			Assembler.StackSizes.Push(4);
		}
	}
}