using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Switch)]
	public class Switch: Op {
		private string[] mLabels;
		public Switch(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			uint[] xCases = aReader.OperandValueBranchLocations;
			mLabels = new string[xCases.Length];
			for (int i = 0; i < xCases.Length; i++) {
			    mLabels[i] = GetInstructionLabel(xCases[i]);
			}
		}

		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.EAX);
			for(int i = 0; i < mLabels.Length; i++){
				new CPUx86.Compare(CPUx86.Registers.EAX, "0" + i.ToString("X") + "h");
				new CPUx86.JumpIfEqual(mLabels[i]);
			}
			Assembler.StackContents.Pop();
		}
	}
}