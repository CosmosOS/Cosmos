using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodHeaderOp: MethodHeaderOp {
		public readonly int[] Locals;
		public readonly string LabelName = "";
		public readonly MethodInformation.Argument[] Args;
		public X86MethodHeaderOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			LabelName = aMethodInfo.LabelName;
			Args = aMethodInfo.Arguments.ToArray();
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
			AssembleHeader(Assembler, LabelName, Locals, Args);
		}

		public static void AssembleHeader(Assembler.Assembler aAssembler, string aLabelName, int[] aLocals, MethodInformation.Argument[] aArguments) {
			new CPU.Label(aLabelName);
			new CPUx86.Push(CPUx86.Registers.EBP);
			//new CPUx86.Move("eax", LdStr.GetContentsArrayName(aAssembler, aLabelName) + "__Contents");
			//new CPUx86.Add("eax", "12");
			//new CPUx86.Push("eax");
			new CPUx86.Move(CPUx86.Registers.EBP, CPUx86.Registers.ESP);
			foreach (int xLocalSize in aLocals) {
				aAssembler.StackSizes.Push(xLocalSize);
				for (int i = 0; i < (xLocalSize / 4); i++) {
					new CPUx86.Pushd("0");
				}
			}
			//new LdStr(aLabelName) {
			//	Assembler = aAssembler
			//}.
			//Assemble();
			//new CPUx86.Add(CPUx86.Registers.ESP, "4");
		}
	}
}