using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodHeaderOp: MethodHeaderOp {
		public override void Assemble(Instruction aInstruction, MethodInformation aMethodInfo) {
			// TODO: add support for variables with a diff datasize, other than 32bit
			Assembler.Add(new CPU.Label(aMethodInfo.LabelName));
			foreach (MethodInformation.Variable xVarDef in aMethodInfo.Locals) {
				Assembler.Add(new CPUx86.Pushd(" 0"));
			}
		}
	}
}