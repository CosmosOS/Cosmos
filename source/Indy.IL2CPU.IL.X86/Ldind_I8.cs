using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldind_I8)]
	public class Ldind_I8: Op {
		public Ldind_I8(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax");
			Assembler.Add(new CPU.Pushd("[eax + 4]"));
			Assembler.Add(new CPU.Pushd("[eax]"));
			Assembler.StackSizes.Push(8);
		}
	}
}