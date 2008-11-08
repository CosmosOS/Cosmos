using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Div)]
	public class Div: Op {
		public Div(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			var xSize = Assembler.StackContents.Pop();
			if (xSize.IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			if (xSize.Size == 8) {
				//TODO: implement proper div support for 8byte values!
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.IDivide { DestinationReg = CPUx86.Registers.ECX };
				//new CPUx86.Push("0");
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };

			} else {
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
				new CPUx86.IDivide{DestinationReg=CPUx86.Registers.ECX};
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
			}
		}
	}
}