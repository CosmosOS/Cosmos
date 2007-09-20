using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldfld)]
	public class Ldfld: Op {
		public Ldfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo, null) {
			FieldDefinition xField = aInstruction.Operand as FieldDefinition;
			if (xField == null) {
				throw new Exception("Field not found!");
			}
			string xFieldId = xField.ToString();
			TypeInformation.Field xTheField = aMethodInfo.TypeInfo.Fields[xFieldId];
			RelativeAddress = xTheField.RelativeAddress;
			FieldSize = xTheField.Size;
			if (FieldSize != 4) {
				throw new NotSupportedException("Field sizes other than 4 bytes are not supported yet!");
			}
		}

		public readonly string RelativeAddress;
		public readonly uint FieldSize;
		public override void DoAssemble() {
			Pop("eax");
			Pushd("[eax " + RelativeAddress + "]");
		}
	}
}