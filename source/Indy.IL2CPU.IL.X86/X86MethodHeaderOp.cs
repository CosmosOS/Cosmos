using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodHeaderOp: MethodHeaderOp {
		public readonly int LocalsCount;
		public readonly string LabelName;
		public X86MethodHeaderOp(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			LabelName = aMethodInfo.LabelName;
			LocalsCount = aMethodInfo.Locals.Length;
		}

		public override void Assemble() {
			// TODO: add support for variables with a diff datasize, other than 32bit
			Assembler.Add(new CPU.Label(LabelName));
			Assembler.Add(new CPUx86.Move("ebp", "esp"));
			for (int i = 0; i < LocalsCount; i++) {
			Assembler.Add(new CPUx86.Pushd("ebp"));
			}
		}
	}
}