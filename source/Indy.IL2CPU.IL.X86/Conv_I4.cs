using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_I4)]
	public class Conv_I4: Op {
		public Conv_I4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			StackContent xSource = Assembler.StackContents.Pop();
			if (xSource.IsFloat) {
				throw new Exception("Floats not yet supported");
			}
			switch (xSource.Size) {
				case 1:
				case 2:
				case 4: {
						break;
					}
				case 8: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Add("esp", "4");
						new CPUx86.Pushd(CPUx86.Registers.EAX);
						break;

					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(4, true, false, true));
		}
	}
}