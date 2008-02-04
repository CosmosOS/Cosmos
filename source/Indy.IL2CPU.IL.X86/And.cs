using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.And)]
	public class And: Op {
		public And(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			StackContent xStackContent = Assembler.StackContents.Peek();
			if (xStackContent.IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
			if (xSize > 8) {
				throw new Exception("StackSize>8 not supported");
			}
			new CPU.Pop(CPU.Registers.EAX);
			if (xSize > 4) {
				new CPU.Add("esp", "4");
			}
			new CPU.Pop(CPU.Registers.EDX);
			if (xSize > 4) {
				new CPU.Add("esp", "4");
			}
			new CPU.And(CPU.Registers.EAX, CPU.Registers.EDX);
			if (xSize > 4) {
				new CPU.Pushd("0");
			}
			new CPU.Pushd(CPU.Registers.EAX);
			Assembler.StackContents.Push(xStackContent);
		}
	}
}