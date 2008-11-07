using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_U4)]
	public class Conv_U4: Op {
		public Conv_U4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			// todo: WARNING: not implemented correctly!
			var xStackItem = Assembler.StackContents.Pop();
			if (xStackItem.IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			switch (xStackItem.Size) {
				case 1:
				case 2: {
						new CPUx86.Noop();
						break;
					}
				case 8: {
						new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						break;
					}
				case 4: {
						new CPUx86.Noop();
						break;
					}
				default:
					throw new Exception("SourceSize " + xStackItem.Size + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		}
	}
}