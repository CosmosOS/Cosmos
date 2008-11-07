using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_I)]
	public class Conv_I: Op {
		public Conv_I(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			var xSource = Assembler.StackContents.Pop();
			switch (xSource.Size) {
				case 1:
				case 2:
				case 4: {
						new CPUx86.Noop();
						break;
					}
				case 8: {
//    					new CPUx86.Pop(CPUx86.Registers_Old.EAX);
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
//						new CPUx86.Pushd(CPUx86.Registers_Old.EAX);
						break;

					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		}
	}
}