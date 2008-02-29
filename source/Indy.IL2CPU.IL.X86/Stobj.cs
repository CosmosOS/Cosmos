using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stobj)]
	public class Stobj: Op {
		public Stobj(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xFieldSize = Assembler.StackContents.Pop().Size;
			Assembler.StackContents.Pop();
			new CPUx86.Move("ecx", "[esp + " + xFieldSize + "]");
			for (int i = 0; i < (xFieldSize / 4); i++) {
				new CPUx86.Pop("eax");
				new CPUx86.Move("dword [ecx + 0x" + (i * 4).ToString("X") + "]", "eax");
			}
			switch (xFieldSize % 4) {
				case 1: {
						new CPUx86.Pop("eax");
						new CPUx86.Move("byte [ecx + " + ((xFieldSize / 4) * 4) + "]", "al");
						break;
					}
				case 2: {
						new CPUx86.Pop("eax");
						new CPUx86.Move("word [ecx + " + ((xFieldSize / 4) * 4) + "]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (xFieldSize % 4) + " not supported!");
			}
			new CPUx86.Add("esp", "4");
		}
	}
}