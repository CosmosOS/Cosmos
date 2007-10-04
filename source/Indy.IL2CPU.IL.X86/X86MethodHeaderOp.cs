using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodHeaderOp: MethodHeaderOp {
		public readonly int LocalsCount = 0;
		public readonly string LabelName = "";
		public X86MethodHeaderOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			LabelName = aMethodInfo.LabelName;
			LocalsCount = aMethodInfo.Locals.Length;
		}

		public override void DoAssemble() {
			// TODO: add support for variables with a diff datasize, other than 32bit
			AssembleHeader(Assembler, LabelName, LocalsCount);
		}

		public static void AssembleHeader(Assembler.Assembler aAssembler, string aLabelName, int aLocalsCount) {
			aAssembler.Add(new CPU.Label(aLabelName));
			aAssembler.Add(new CPUx86.Push("ebp"));
			aAssembler.Add(new CPUx86.Move("ebp", "esp"));
			for (int i = 0; i < aLocalsCount; i++) {
				aAssembler.Add(new CPUx86.Pushd("0"));
			}
		}
	}
}