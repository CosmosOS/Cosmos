using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Sub)]
	public class Sub: Op {
		public Sub(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.ECX);
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Sub(CPUx86.Registers.EAX, CPUx86.Registers.ECX);
			new CPUx86.Push(CPUx86.Registers.EAX);
			Assembler.StackContents.Pop();
		}
	}
}