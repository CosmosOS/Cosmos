using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Add)]
	public class Add: Op {
		public Add(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo, null) {
		}
		public override void DoAssemble() {
			Assembler.Add(new CPU.Pop("eax"));
			Assembler.Add(new CPU.Add("eax", "[esp]"));
			Assembler.Add(new CPU.Add("esp", "4"));
			Pushd("eax");
		}
	}
}