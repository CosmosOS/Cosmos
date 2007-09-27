using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldfld)]
	public class Ldfld: Op {
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
			TypeInformation.Field xTheField;
			uint xStorageSize;
			xTheField = Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xField.DeclaringType), out xStorageSize)[xFieldId];
			RelativeAddress = xTheField.RelativeAddress;
			FieldSize = xTheField.Size;
			if (FieldSize == 1 || FieldSize == 2 || FieldSize == 4 || FieldSize == 8)
				return;
			System.Diagnostics.Debugger.Break();
			throw new NotSupportedException("FieldSizes other than 1, 2, 4 and 8 are not supported yet! (" + FieldSize + ")");
		}

		// todo: implement correct support for different field sizes

		public readonly string RelativeAddress;
		public readonly uint FieldSize;
		public override void DoAssemble() {
			Pop("eax");
			string xPushPrefix = "";
			switch(FieldSize) {
				case 1:
					xPushPrefix = "byte";
					break;
				case 2:
					xPushPrefix = "word";
					break;
				case 4:
					xPushPrefix = "dword";
					break;
				case 8:
					xPushPrefix = "qword";
					break;
			}
			Push(Assembler, xPushPrefix + " [eax " + RelativeAddress + "]");
		}
	}
}