using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Asm = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Call)]
	public class Call: Op {
		public void Assemble(string aMethod) {
			Call(aMethod);
		}

		public override void Assemble(Instruction aInstruction) {
            Assemble((new Asm.Label((MethodDefinition)aInstruction.Operand)).Name);
		}
	}
}