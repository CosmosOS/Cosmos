using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Div_Un)]
	public class Div_Un: Op {
		public Div_Un(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Math.Max(Assembler.StackSizes.Pop(), Assembler.StackSizes.Pop());
			new CPU.Pop("ecx");
			new CPU.Pop("eax");
			new CPU.Xor("edx", "edx");
			new CPU.Divide("ecx");
			new CPU.Pushd("eax");
			Assembler.StackSizes.Push(xSize);
		}
	}
}