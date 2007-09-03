using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Asm = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Call)]
	public class Call: Op {
		public readonly string LabelName;
		public Call(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			LabelName = new Asm.Label((MethodReference)aInstruction.Operand).Name;
		}
		public void Assemble(string aMethod) {
			Call(aMethod);
		}

		public override void Assemble() {
			Assemble(LabelName);
		}
	}
}