using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stfld)]
	public class Stfld: Op {
		private readonly TypeInformation.Field mField;
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
				FieldReference xFieldRef = aInstruction.Operand as FieldReference;
				if (xFieldRef == null) {
					string typeName = aInstruction.Operand == null ? "" : aInstruction.Operand.GetType().FullName;
					throw new Exception("Field not found! (Operand = '" + (aInstruction.Operand ?? "**NULL**") + "'[" + typeName + "])");
				}
				xField = Engine.GetDefinitionFromFieldReference(xFieldRef);
			}
			string xFieldId = xField.ToString();
			int xStorageSize;
			SortedList<String, TypeInformation.Field> xFieldInfo = Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xField.DeclaringType), out xStorageSize);
			if (!xFieldInfo.ContainsKey(xFieldId)) {
				throw new Exception("Field not found!");
			}
			mField = xFieldInfo[xFieldId];
		}

		public override void DoAssemble() {
			Stfld(Assembler, mField);
		}
	}
}