using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldfld)]
	public class Ldfld: Op {
		private readonly TypeInformation.Field mField;
		public Ldfld(TypeInformation.Field aField):base(null, null) {
			mField = aField;
		}
		public Ldfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
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
			int xStorageSize;
			mField = Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xField.DeclaringType), out xStorageSize)[xFieldId];
		}

		public override void DoAssemble() {
			Ldfld(Assembler, mField);
		}
	}
}