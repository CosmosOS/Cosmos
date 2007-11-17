using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Shr)]
	public class Shr: Op {
		public Shr(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			new CPU.Pop("eax"); // shift amount
			int xSize = Assembler.StackSizes.Pop();
			new CPU.Pop("edx"); // value
			xSize = Math.Max(xSize, Assembler.StackSizes.Pop());
			new CPU.Move("cl", "al");
			new CPU.Move("ebx", "0");
			new CPU.ShiftRight("edx", "ebx", "cl");
			new CPU.Pushd("edx");
			Assembler.StackSizes.Push(xSize);
		}
	}
}