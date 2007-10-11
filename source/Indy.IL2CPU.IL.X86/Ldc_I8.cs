using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldc_I8)]
	public class Ldc_I8: Op {
		private readonly long mValue;
		public Ldc_I8(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			mValue = Int64.Parse(aInstruction.Operand.ToString());
		}
		public override void DoAssemble() {
			string theValue = mValue.ToString("X");
			Assembler.Add(new CPU.Pushd("0" + theValue.Substring(0, 8) + "h"));
			Assembler.Add(new CPU.Pushd("0" + theValue.Substring(8) + "h"));
			Assembler.StackSizes.Push(8);
		}
	}
}