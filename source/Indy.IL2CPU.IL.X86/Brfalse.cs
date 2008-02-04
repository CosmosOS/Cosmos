using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Brfalse)]
	public class Brfalse: Op {
		public readonly string TargetLabel;
		public Brfalse(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		}

		public override void DoAssemble() {
			new CPUx86.Popd(CPUx86.Registers.EAX);
			var xStackContent = Assembler.StackContents.Pop();
			if (xStackContent.IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			if (xStackContent.Size > 8) {
				throw new Exception("StackSize>8 not supported");
			}
			if (xStackContent.Size > 4) {
				new CPUx86.Add("esp", "4");
			}
			new CPUx86.Compare(CPUx86.Registers.EAX, "0");
			new CPUx86.JumpIfEquals(TargetLabel);
		}
	}
}