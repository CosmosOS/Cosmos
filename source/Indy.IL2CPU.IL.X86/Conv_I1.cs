using System;
using System.IO;
using CPU = Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_I1)]
	public class Conv_I1: Op {
		public Conv_I1(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			StackContent xSource = Assembler.StackContents.Pop();
			if (xSource.IsFloat)
			{
				throw new Exception("Floats not yet supported");
			}
			switch (xSource.Size)
			{
			case 1:
				new CPUx86.Noop();
				break;
			case 2:
			case 4:
				new CPUx86.Pop("eax");
				new CPUx86.MoveAndSignExtend("eax", "al");
				new CPUx86.Push("eax");
				break;
			case 8:
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Pop("EBX");
				new CPUx86.MoveAndSignExtend("eax", "al");
				new CPUx86.Pushd(CPUx86.Registers.EAX);
				break;
			default:
				throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(1, true, false, true));
		}
	}
}