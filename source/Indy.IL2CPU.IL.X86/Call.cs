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
		public readonly bool HasResult;
		public Call(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo, null) {
			HasResult = !((MethodReference)aInstruction.Operand).ReturnType.ReturnType.FullName.Contains("System.Void");
			LabelName = new Asm.Label((MethodReference)aInstruction.Operand).Name;
		}
		public void Assemble(string aMethod) {
			Call(aMethod);
			if (HasResult) {
				Push(Assembler, "eax");
			}
		}

		public override void DoAssemble() {
			Assemble(LabelName);
		}
	}
}