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
			int xStorageSize;
			xTheField = Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xField.DeclaringType), out xStorageSize)[xFieldId];
			RelativeAddress = xTheField.RelativeAddress;
			FieldSize = Engine.GetFieldStorageSize(xField.FieldType);
			
			if (FieldSize == 1 || FieldSize == 2 || FieldSize == 4)
				return;
			throw new NotSupportedException("FieldSizes other than 1, 2, 4 are not supported yet! (" + FieldSize + ")");
		}

		// todo: implement correct support for different field sizes

		public readonly string RelativeAddress;
		public readonly int FieldSize;
		public override void DoAssemble() {
			Pop("ecx");
			string xPushPrefix = "";
			string xRegister = "";
			switch (FieldSize) {
				case 1:
					xPushPrefix = "byte";
					xRegister = "al";
					break;
				case 2:
					xPushPrefix = "word";
					xRegister = "ax";
					break;
				case 4:
					xPushPrefix = "dword";
					xRegister = "eax";
					break;
			}
			Assembler.StackSizes.Pop();
			Assembler.StackSizes.Push(FieldSize);
			Move(Assembler, "eax", "0");
			Move(Assembler, xPushPrefix + " " + xRegister, "[ecx " + RelativeAddress + "]");
			Push(Assembler, FieldSize, "eax");
		}
	}
}