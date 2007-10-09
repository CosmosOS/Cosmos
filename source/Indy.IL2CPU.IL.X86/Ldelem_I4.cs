using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldelem_I4, true)]
	public class Ldelem_I4: Op {
		public Ldelem_I4(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		public override void DoAssemble() {
			Ldelem_Any.Assemble(Assembler, 4);
		}
	}
}