using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_U1)]
	public class Conv_U1: Op {
		public Conv_U1(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats not supported yet!");
			}
			int xSource = Assembler.StackContents.Pop().Size;
			switch (xSource) {
				case 2:
				case 4: {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						break;
					}
				case 8: {
						new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
						new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
						break;
					}
				case 1: {
						new CPUx86.Noop();
						break;
					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(1, typeof(byte)));
		}
	}
}