using System;
using System.IO;

using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Brfalse)]
	public class Brfalse: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Brfalse(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
		}

		public override void DoAssemble() {
			var xStackContent = Assembler.StackContents.Pop();
			if (xStackContent.IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			if (xStackContent.Size > 8) {
				throw new Exception("StackSize>8 not supported");
			}

			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";

			if (xStackContent.Size > 4)
			{
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };
                new CPUx86.Jump { DestinationLabel = TargetLabel };
				new CPU.Label(LabelFalse);
			} else
			{
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = TargetLabel };
			}
		}
	}
}