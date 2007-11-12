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
			aAssembler.Add(new CPU.Comment("; address at: [esp + " + aSize + "]"));
			int xStorageSize = aSize;
			if (xStorageSize < 4) {
				xStorageSize = 4;
			}
			aAssembler.Add(new CPUx86.Move("ebx", "[esp + " + xStorageSize + "]"));
			for (int i = 0; i < (aSize / 4); i++) {
				Move(aAssembler, "eax", "[esp + " + (i * 4) + "]");
				Move(aAssembler, "[ebx + " + (i * 4) + "]", "eax");
			}
			switch (aSize % 4) {
				case 0: {
						break;
					}
				case 1: {
						Move(aAssembler, "eax", "[esp + " + ((aSize / 4) * 4) + "]");
						Move(aAssembler, "[ebx + " + ((aSize / 4) * 4) + "]", "al");
						break;
					}
				case 2: {
						Move(aAssembler, "eax", "[esp + " + ((aSize / 4) * 4) + "]");
						Move(aAssembler, "[ebx + " + ((aSize / 4) * 4) + "]", "ax");
						break;
					}
				case 3: {
						Move(aAssembler, "eax", "[esp + " + ((aSize / 4) * 4) + "]");
						Move(aAssembler, "[ebx + " + ((aSize / 4) * 4) + "]", "ax");
						Move(aAssembler, "eax", "[esp + " + (((aSize / 4) * 4) + 2) + "]");
						Move(aAssembler, "[ebx + " + (((aSize / 4) * 4) + 2) + "]", "al");
						break;
					}
				default:
					throw new Exception("Error, shouldn't occur");
			}
			aAssembler.Add(new CPUx86.Add("esp", "0x" + (xStorageSize + 4).ToString("X")));
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Pop();
		}

		public override void DoAssemble() {
			Assemble(Assembler, 4);
		}
	}
}