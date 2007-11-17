using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Rem)]
	public class Rem: Op {
		public Rem(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Math.Max(Assembler.StackSizes.Pop(), Assembler.StackSizes.Pop());
			new CPU.Pop("ecx");
			new CPU.Pop("eax"); // gets devised by ecx
			new CPU.Xor("edx", "edx");
			new CPU.Divide("ecx");
			new CPU.Pushd("edx");
			Assembler.StackSizes.Push(xSize);
		}
	}
}