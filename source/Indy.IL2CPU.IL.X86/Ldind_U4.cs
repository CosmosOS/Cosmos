using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldind_U4)]
	public class Ldind_U4: Op {
		public Ldind_U4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Pushd(CPUx86.Registers.AtEAX);
		}
	}
}