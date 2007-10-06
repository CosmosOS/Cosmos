using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_U2)]
	public class Conv_U2: Op {
		public Conv_U2(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("ecx");
			Move(Assembler, "eax", "0");
			Move(Assembler, "ax", "cx");
			Pushd("eax");
		}
	}
}