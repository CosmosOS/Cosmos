using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsflda)]
	public class Ldsflda: Op {
		private readonly string mDataName;

		public Ldsflda(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			Engine.QueueStaticField(xField, out mDataName);
		}

		public override void DoAssemble() {
			Pushd(4, mDataName);
		}
	}
}