using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Neg)]
	public class Neg: Op {
		public Neg(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
			new CPUx86.Neg{DestinationReg=CPUx86.Registers.EAX};
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		}
	}
}