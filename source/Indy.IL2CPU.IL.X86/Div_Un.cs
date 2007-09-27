using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Div_Un)]
	public class Div_Un: Op {
		public Div_Un(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("ecx");
			Pop("eax");
			Assembler.Add(new CPU.Divide("ecx"));
			Pushd("eax");
		}
	}
}