using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Sub)]
	public class Sub: Op {
		public Sub(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void Assemble() {
			Assembler.Add(new CPU.Pop("eax"));
			Assembler.Add(new CPU.Sub("eax", "[esp]"));
			Assembler.Add(new CPU.Add("esp", "4"));
			Assembler.Add(new CPU.Move("[esp]", "eax"));
		}
	}
}