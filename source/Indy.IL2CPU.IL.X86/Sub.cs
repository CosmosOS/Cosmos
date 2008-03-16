using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Sub)]
	public class Sub: Op {
		public Sub(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			var stackTop = Assembler.StackContents.Pop();

			if (stackTop.IsFloat)
				throw new NotImplementedException("floating-point arithmetics");

			switch (stackTop.Size) {
				case 1:
				case 2:
				case 4:
					new CPUx86.Pop(CPUx86.Registers.ECX);
					new CPUx86.Pop(CPUx86.Registers.EAX);
					new CPUx86.Sub(CPUx86.Registers.EAX, CPUx86.Registers.ECX);
					new CPUx86.Push(CPUx86.Registers.EAX);
					break;
				case 8:
					new CPUx86.Pop(CPUx86.Registers.EAX);
					new CPUx86.Pop(CPUx86.Registers.EDX);
					new CPUx86.Sub("[esp]", CPUx86.Registers.EAX);
					new CPUx86.SubWithCarry("[esp + 4]", CPUx86.Registers.EDX);
					break;
				default:
					throw new NotSupportedException(string.Format("sub operationd doesn't support size {0}", stackTop.Size));
			}
		}
	}
}