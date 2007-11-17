using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Shl)]
	public class Shl: Op {
		public Shl(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Math.Max(Assembler.StackSizes.Pop(), Assembler.StackSizes.Pop());
			new CPU.Pop("eax"); // shift amount
			new CPU.Pop("edx"); // value
			new CPU.Move("ebx", "0");
			new CPU.Move("cl", "al");
			new CPU.ShiftLeft("edx", "ebx", "cl");
			new CPU.Pushd("edx");
			Assembler.StackSizes.Push(xSize);
		}
	}
}