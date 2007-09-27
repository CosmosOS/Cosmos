using System;
using System.Linq;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldc_R4)]
	public class Ldc_R4: Op {
		private Single mValue;
		public Ldc_R4(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			mValue = (Single)aInstruction.Operand;
		}
		public override void DoAssemble() {
			Pushd("0" + BitConverter.GetBytes(mValue).Aggregate("", (x, b) => x + b.ToString("X2")) + "h");
		}
	}
}