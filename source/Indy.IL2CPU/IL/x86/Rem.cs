using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Rem)]
	public class Rem: Op {
		public Rem(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats not yet supported");
			}
			StackContent xStackItem = Assembler.StackContents.Peek();
			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
			if (xSize > 4) {
				new CPUx86.Pop(CPUx86.Registers.ECX);
				new CPUx86.Add("esp", "4");
				new CPUx86.Pop(CPUx86.Registers.EAX); // gets devised by ecx
				new CPUx86.Xor(CPUx86.Registers.EDX, CPUx86.Registers.EDX);

				new CPUx86.Divide(CPUx86.Registers.ECX); // => EAX / ECX 
				new CPUx86.Pushd(CPUx86.Registers.EDX);

			} else {
				new CPUx86.Pop(CPUx86.Registers.ECX);
				new CPUx86.Pop(CPUx86.Registers.EAX); // gets devised by ecx
				new CPUx86.Xor(CPUx86.Registers.EDX, CPUx86.Registers.EDX);

				new CPUx86.Divide(CPUx86.Registers.ECX); // => EAX / ECX 
				new CPUx86.Pushd(CPUx86.Registers.EDX);
			}
			Assembler.StackContents.Push(xStackItem);
		}
	}
}