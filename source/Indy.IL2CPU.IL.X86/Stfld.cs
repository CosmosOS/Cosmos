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
			TypeInformation.Field xTheField;
			uint xStorageSize;
			SortedList<String, TypeInformation.Field> xFieldInfo = Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xField.DeclaringType), out xStorageSize);
			if(!xFieldInfo.ContainsKey(xFieldId)) {
				Console.WriteLine("Field not Found: '{0}'", xFieldId);
				Console.WriteLine("Running On Type: " + xField.DeclaringType.FullName);
				Console.WriteLine("AvailableFields:");
				foreach (string s in xFieldInfo.Keys) {
					Console.WriteLine("\t" + s);
				}
			}
			xTheField = xFieldInfo[xFieldId];
			RelativeAddress = xTheField.RelativeAddress;
			FieldSize = xTheField.Size;
			if (FieldSize == 1 || FieldSize == 2 || FieldSize == 4)
				return;
			System.Diagnostics.Debugger.Break();
			throw new NotSupportedException("FieldSizes other than 1, 2, 4 and 8 are not supported yet! (" + FieldSize + ")");
		}

		public readonly string RelativeAddress;
		public readonly uint FieldSize;

		public override void DoAssemble() {
			Pop("eax"); // new value
			Pop("ecx"); // instance
			string xMovePrefix = "";
			string xRegister = "";
			switch (FieldSize) {
				case 1:
					xMovePrefix = "byte";
					xRegister = "al";
					break;
				case 2:
					xMovePrefix = "word";
					xRegister = "ax";
					break;
				case 4:
					xMovePrefix = "dword";
					xRegister = "eax";
					break;
			}
			Move(Assembler, xMovePrefix + " [ecx " + RelativeAddress + "]", xRegister);
		}
	}
}