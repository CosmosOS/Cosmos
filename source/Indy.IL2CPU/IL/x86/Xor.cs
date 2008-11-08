using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Xor)]
	public class Xor: Op {
		public Xor(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats not supported");
			}
			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
			new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
			Assembler.StackContents.Push(new StackContent(xSize));
		}
	}
}