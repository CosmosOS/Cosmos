using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Shl)]
	public class Shl: Op {
		public Shl(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax"); // shift amount
			Pop("edx"); // value
			Assembler.Add(new CPU.ShiftLeft("edx", "0", "eax"));
			Pushd("edx");
		}
	}
}