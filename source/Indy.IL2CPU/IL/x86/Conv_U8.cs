using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_U8)]
	public class Conv_U8: Op {
		public Conv_U8(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats are not yet supported");
			}
			int xSource = Assembler.StackContents.Peek().Size;
			switch (xSource) {
				case 1:
				case 2:
				case 4: {
						Assembler.StackContents.Pop();
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationValue = 0 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						Assembler.StackContents.Push(new StackContent(8, typeof(ulong)));
						break;
					}
				case 8: {
						new CPUx86.Noop();
						break;
					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
		}
	}
}