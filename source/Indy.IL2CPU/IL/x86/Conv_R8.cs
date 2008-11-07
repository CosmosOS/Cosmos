using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_R8)]
	public class Conv_R8: Op {
		public Conv_R8(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			var xStackItem = Assembler.StackContents.Peek();
			switch (xStackItem.Size) {
				case 1:
				case 2:
				case 4: {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationValue = 0 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						Assembler.StackContents.Pop();
						Assembler.StackContents.Push(new StackContent(8, typeof(Double)));

						break;
					}
				case 8: {
						new CPUx86.Noop();
						break;
					}
				default:
					throw new Exception("SourceSize " + xStackItem.Size + " not supported!");
			}
		}
	}
}