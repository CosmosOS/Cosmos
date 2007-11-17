using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Div)]
	public class Div: Op {
		public Div(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Assembler.StackSizes.Pop();
			new CPU.Pop("ecx");
			new CPU.Pop("eax");
			new CPU.Divide("ecx");
			new CPU.Pushd("eax");
			Assembler.StackSizes.Pop();
		}
	}
}