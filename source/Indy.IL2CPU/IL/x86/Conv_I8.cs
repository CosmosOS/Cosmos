using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_I8)]
	public class Conv_I8: Op {
		public Conv_I8(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			var xSource = Assembler.StackContents.Pop();
			switch (xSource.Size) {
				case 1:
				case 2:
				case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.SignExtendAX { Size = 32 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
				case 8:
					new CPUx86.Noop();
					break;
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(8, true, false, true));
		}
	}
}