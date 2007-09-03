using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;
using Asm = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Newobj)]
	public class Newobj: Op {
		public readonly string CtorName;
		public Newobj(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			CtorName = new Asm.Label(((MethodReference)aInstruction.Operand).Name).Name;
		}
		
		public override void Assemble() {
			Call(CtorName);
		}
	}
}