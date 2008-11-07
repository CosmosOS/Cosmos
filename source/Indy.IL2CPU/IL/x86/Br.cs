using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Br)]
	public class Br: Op {
		private readonly string mTargetInstructionName;
		public Br(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mTargetInstructionName = GetInstructionLabel(aReader.OperandValueBranchPosition);
		}
		public override void DoAssemble() {
            new CPU.Jump { DestinationLabel = mTargetInstructionName };
		}
	}
}