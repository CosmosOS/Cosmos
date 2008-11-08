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
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };

                new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };

			} else {
				new CPUx86.Pop{DestinationReg = CPUx86.Registers.ECX};
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };

                new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
			}
			Assembler.StackContents.Push(xStackItem);
		}
	}
}