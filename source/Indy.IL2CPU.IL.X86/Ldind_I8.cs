using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldind_I8)]
	public class Ldind_I8: Op {
		public Ldind_I8(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			Assembler.StackContents.Pop();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Pushd("[eax + 4]");
			new CPUx86.Pushd(CPUx86.Registers.AtEAX);
			Assembler.StackContents.Push(new StackContent(8, typeof(long)));
		}
	}
}