using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stobj)]
	public class Stobj: Op {
		public Stobj(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			//new CPUx86.Pop(CPUx86.Registers.EDX);
			//new CPUx86.Pop(CPUx86.Registers.EAX);
			//new CPUx86.Move(CPUx86.Registers.AtEAX, CPUx86.Registers.EDX);
			int xFieldSize = Assembler.StackSizes.Pop();
			Assembler.StackSizes.Pop();
			new CPUx86.Pop("ecx");
			for (int i = 0; i < (xFieldSize / 4); i++) {
				new CPUx86.Pop("eax");
				new CPUx86.Move("dword [ecx + 0x" + (xFieldSize - (i * 4)).ToString("X") + "]", "eax");
			}
			switch (xFieldSize % 4) {
				case 1: {
						new CPUx86.Pop("eax");
						new CPUx86.Move("byte [ecx]", "al");
						break;
					}
				case 2: {
						new CPUx86.Pop("eax");
						new CPUx86.Move("word [ecx]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (xFieldSize % 4) + " not supported!");

			}
		}
	}
}