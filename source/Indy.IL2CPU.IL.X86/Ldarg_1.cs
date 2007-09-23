using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldarg_1)]
	public class Ldarg_1: Ldarg {
		public Ldarg_1(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(null, aMethodInfo) {
			SetArgIndex(1, aMethodInfo);
		}
	}
}