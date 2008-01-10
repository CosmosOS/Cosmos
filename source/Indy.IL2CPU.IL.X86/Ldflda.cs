using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldflda)]
	public class Ldflda: Op {
		private TypeInformation mType;
		private TypeInformation.Field mField;

		public Ldflda(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldDefinition xField = aInstruction.Operand as FieldDefinition;
			if (xField == null) {
				FieldReference xFieldRef = aInstruction.Operand as FieldReference;
				if (xFieldRef == null) {
					string typeName = aInstruction.Operand == null ? "" : aInstruction.Operand.GetType().FullName;
					throw new Exception("Field not found! (Operand = '" + (aInstruction.Operand ?? "**NULL**") + "'[" + typeName + "])");
				}
				xField = Engine.GetDefinitionFromFieldReference(xFieldRef);
			}
			string xFieldId = xField.ToString();
			mType = Engine.GetTypeInfo(Engine.GetDefinitionFromTypeReference(xField.DeclaringType));
			mField = mType.Fields[xFieldId];
		}

		public override void DoAssemble() {
			Ldflda(Assembler, mType, mField);
		}
	}
}