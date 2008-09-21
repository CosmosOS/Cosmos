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
			if (xSize > 4)
			{
				new CPU.Pop(CPU.Registers.EAX);
				new CPU.Pop("ebx");
				new CPU.Pop(CPU.Registers.EDX);
				new CPU.Pop("ecx");
				new CPU.And(CPU.Registers.EAX, CPU.Registers.EDX);
				new CPU.And("ebx", "ecx");
				new CPU.Pushd("ebx");
				new CPU.Pushd(CPU.Registers.EAX);
			}else
			{
				new CPU.Pop(CPU.Registers.EAX);
				new CPU.Pop(CPU.Registers.EDX);
				new CPU.And(CPU.Registers.EAX, CPU.Registers.EDX);
				new CPU.Pushd(CPU.Registers.EAX);
			}
			Assembler.StackContents.Push(xStackContent);
		}
	}
}