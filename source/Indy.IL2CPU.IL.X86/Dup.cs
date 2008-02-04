using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Dup)]
	public class Dup: Op {
		public Dup(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			var xStackContent = Assembler.StackContents.Peek();
			for (int i = 0; i < ((xStackContent.Size / 4) + (xStackContent.Size % 4 == 0 ? 0 : 1)) ; i++) {
				new CPUx86.Move(CPUx86.Registers.EAX, "[esp " + (i > 0 ? "+" + (i * 4) : "") + "]");
				new CPUx86.Pushd(CPUx86.Registers.EAX);
			}
			Assembler.StackContents.Push(xStackContent);
		}
	}
}