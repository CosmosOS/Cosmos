using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Switch)]
	public class Switch: Op {
		private string[] mLabels;
		public Switch(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			Instruction[] xCases = (Instruction[])aInstruction.Operand;
			mLabels = new string[xCases.Length];
			for (int i = 0; i < xCases.Length; i++) {
				mLabels[i] = GetInstructionLabel(xCases[i]);
			}
		}

		public override void DoAssemble() {
			Pop("eax");
			for(int i = 0; i < mLabels.Length; i++){
				Compare("eax", "0" + i.ToString("X") + "h");
				JumpIfEquals(mLabels[i]);
			}
			Assembler.StackSizes.Pop();
		}
	}
}