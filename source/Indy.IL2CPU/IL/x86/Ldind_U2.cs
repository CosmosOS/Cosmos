using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldind_U2)]
	public class Ldind_U2: Op {
		public Ldind_U2(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			Assembler.StackContents.Pop();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Move(CPUx86.Registers.EDX, "0");
			new CPUx86.Move(CPUx86.Registers.DX, CPUx86.Registers.AtEAX);
			new CPUx86.Pushd(CPUx86.Registers.EDX);
			Assembler.StackContents.Push(new StackContent(2, typeof(ushort)));
		}
	}
}