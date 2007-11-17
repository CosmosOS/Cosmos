using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldlen, true)]
	public class Ldlen: Op {
		public Ldlen(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Assembler.StackSizes.Pop();
			new CPU.Pop("eax");
			new CPU.Add("eax", "8");
			new CPU.Pushd("[eax]");
			Assembler.StackSizes.Push(4);
		}
	}
}