using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Leave)]
	public class Leave: Op {public readonly string TargetLabel;
	public Leave(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		}
		public override void DoAssemble() {
        new CPU.Jump { DestinationLabel = TargetLabel };
		}
	}
}