using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Asm = Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Call)]
	public class Call: Op {
		public void Assemble(string aMethod) {
			new CPU.Call(aMethod);
		}

		public override void Assemble(Instruction aInstruction) {
            Assemble((new Asm.Label((MethodDefinition)aInstruction.Operand)).Name);
		}
	}
}