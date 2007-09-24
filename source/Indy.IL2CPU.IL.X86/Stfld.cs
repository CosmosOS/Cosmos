using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stfld)]
	public class Stfld: Op {
		public Stfld(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aInstruction == null) {
				throw new ArgumentNullException("aInstruction");
			}
			if (aMethodInfo == null) {
				throw new ArgumentNullException("aMethodInfo");
			}
			FieldDefinition xField = aInstruction.Operand as FieldDefinition;
			if (xField == null) {
				throw new Exception("Field not found!");
			}
			string xFieldId = xField.ToString();
			TypeInformation.Field xTheField;
			uint xStorageSize;
			xTheField = Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xField.DeclaringType), out xStorageSize)[xFieldId];
			RelativeAddress = xTheField.RelativeAddress;
			FieldSize = xTheField.Size;
			if (FieldSize != 4) {
				throw new NotSupportedException("Field sizes other than 4 bytes are not supported yet!");
			}
		}

		public readonly string RelativeAddress;
		public readonly uint FieldSize;

		public override void DoAssemble() {
			Pop("eax"); // new value
			Pop("ecx"); // instance
			Move(Assembler, "[ecx " + RelativeAddress + "]", "eax");
		}
	}
}