using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stind_I)]
	public class Stind_I: Op {
		public Stind_I(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public static void Assemble(Assembler.Assembler aAssembler, int aSize) {
			new CPU.Comment("address at: [esp + " + aSize + "]");
			int xStorageSize = aSize;
			if (xStorageSize < 4) {
				xStorageSize = 4;
			}
			new CPUx86.Move(CPUx86.Registers.EBX, "[esp + " + xStorageSize + "]");
			for (int i = 0; i < (aSize / 4); i++) {
				new CPUx86.Move(CPUx86.Registers.EAX, "[esp + " + (i * 4) + "]");
				new CPUx86.Move("[ebx + " + (i * 4) + "]", "eax");
			}
			switch (aSize % 4) {
				case 0: {
						break;
					}
				case 1: {
						new CPUx86.Move(CPUx86.Registers.EAX, "[esp + " + ((aSize / 4) * 4) + "]");
						new CPUx86.Move("[ebx + " + ((aSize / 4) * 4) + "]", "al");
						break;
					}
				case 2: {
						new CPUx86.Move(CPUx86.Registers.EAX, "[esp + " + ((aSize / 4) * 4) + "]");
						new CPUx86.Move("[ebx + " + ((aSize / 4) * 4) + "]", "ax");
						break;
					}
				case 3: {
						new CPUx86.Move(CPUx86.Registers.EAX, "[esp + " + ((aSize / 4) * 4) + "]");
						new CPUx86.Move("[ebx + " + ((aSize / 4) * 4) + "]", "ax");
						new CPUx86.Move(CPUx86.Registers.EAX, "[esp + " + (((aSize / 4) * 4) + 2) + "]");
						new CPUx86.Move("[ebx + " + (((aSize / 4) * 4) + 2) + "]", "al");
						break;
					}
				default:
					throw new Exception("Error, shouldn't occur");
			}
			new CPUx86.Add("esp", "0x" + (xStorageSize + 4).ToString("X"));
			aAssembler.StackContents.Pop();
			aAssembler.StackContents.Pop();
		}

		public override void DoAssemble() {
			Assemble(Assembler, 4);
		}
	}
}