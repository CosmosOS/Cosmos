using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodHeaderOp: MethodHeaderOp {
		public readonly int[] Locals;
		public readonly string LabelName = "";
		public X86MethodHeaderOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			LabelName = aMethodInfo.LabelName;
			Locals = new int[aMethodInfo.Locals.Length];
			for (int i = 0; i < aMethodInfo.Locals.Length; i++) {
				var xVar = aMethodInfo.Locals[i];
				Locals[i] = xVar.Size;
				if (xVar.Size % 4 != 0) {
					throw new Exception("Local Variable size is not a a multiple of 4");
				}
			}
		}

		public override void DoAssemble() {
			// TODO: add support for variables with a diff datasize, other than 32bit
			AssembleHeader(Assembler, LabelName, Locals);
		}

		public static void AssembleHeader(Assembler.Assembler aAssembler, string aLabelName, int[] aLocals) {
			aAssembler.Add(new CPU.Label(aLabelName));
			aAssembler.Add(new CPUx86.Push("ebp"));
			aAssembler.Add(new CPUx86.Move("ebp", "esp"));
			foreach (int xLocalSize in aLocals) {
				aAssembler.StackSizes.Push(xLocalSize);
				for (int i = 0; i < (xLocalSize / 4); i++) {
					aAssembler.Add(new CPUx86.Pushd("0"));
				}
			}
		}
	}
}