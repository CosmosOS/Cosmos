using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stind_I)]
	public class Stind_I: Op {
		public Stind_I(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public static void Assemble(Assembler.Assembler aAssembler, int aSize) {
			aAssembler.Add(new CPU.Literal("; address at: [esp + " + aSize + "]"));
			aAssembler.Add(new CPUx86.Move("ebx", "[esp + " + aSize + "]"));
			for (int i = 0; i < (aSize / 4); i++) {
				Move(aAssembler, "eax", "[esp + " + (i * 4) + "]");
				Move(aAssembler, "[ebx + " + (i * 4) + "]", "eax");
			}
			aAssembler.Add(new CPUx86.Add("esp", "0x" + (aSize + 4).ToString("X")));
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Pop();
		}

		public override void DoAssemble() {
			Assemble(Assembler, 4);
		}
	}
}