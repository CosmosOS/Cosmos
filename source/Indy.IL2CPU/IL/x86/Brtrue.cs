using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Brtrue)]
	public class Brtrue: Op {
		public readonly string TargetLabel;
		public Brtrue(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		}

		public override void DoAssemble() {
			int xSize = Assembler.StackContents.Pop().Size;
			if (xSize > 8) {
				throw new Exception("StackSize>8 not supported");
			}
			if (xSize > 4)
			{
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = TargetLabel };
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = TargetLabel };
			} else
			{
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = TargetLabel };
			}
		}
	}
}