using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsflda)]
	public class Ldsflda: Op {
		private readonly string mDataName;

		public Ldsflda(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			DoQueueStaticField(xField.DeclaringType.Module.Assembly.Name.Name, xField.DeclaringType.FullName, xField.Name, out mDataName);
		}

		public override void DoAssemble() {
			Pushd(mDataName);
		}
	}
}