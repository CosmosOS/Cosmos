using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldlen)]
	public class Ldlen: Op {
		public Ldlen(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			Assembler.StackContents.Pop();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Add(CPUx86.Registers.EAX, "8");
			new CPUx86.Pushd(CPUx86.Registers.AtEAX);
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		}
	}
}