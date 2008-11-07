using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_U)]
	public class Conv_U: Op {
		public Conv_U(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			var xStackContent = Assembler.StackContents.Pop();
			switch (xStackContent.Size) {
				case 1:
				case 2: {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						break;
					}
				case 8: {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						break;
					}
				case 4: {
						new CPUx86.Noop();
						break;
					}
				default:
					throw new Exception("SourceSize " + xStackContent.Size + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		}
	}
}