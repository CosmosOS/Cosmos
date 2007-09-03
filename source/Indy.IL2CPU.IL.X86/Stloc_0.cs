using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stloc_0)]
	public class Stloc_0: Op {
		public override void Assemble(Instruction aInstruction, MethodInformation aMethodInfo) {
			Pop("eax");
			Move("[esp+0]", "eax");
			//Move("eax", "[esp + " + (aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Offset + aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Size + 4) + "]");
			//Move("[esp + " + (aMethodInfo.Locals[0].Offset + 4) + "]", "eax");
		}
	}
}