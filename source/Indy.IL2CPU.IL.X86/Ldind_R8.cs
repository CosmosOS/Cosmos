using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldind_R8)]
	public class Ldind_R8: Op {
		public Ldind_R8(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax");
			Pushd("[eax + 4]");
			Pushd("[eax]");
			Assembler.StackSizes.Pop();
			Assembler.StackSizes.Push(8);
		}
	}
}