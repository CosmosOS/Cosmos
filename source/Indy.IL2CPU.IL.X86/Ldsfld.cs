using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsfld)]
	public class Ldsfld: Op {
		private string mDataName;
		private bool mIsReferenceTypeOrStruct;
		private int mSize;

		private static bool IsPrimitive(TypeDefinition aType) {
			switch (aType.FullName) {
				case "System.SByte":
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.Byte":
				case "System.UInt16":
				case "System.UInt32":
				case "System.UInt64":
				case "System.Boolean":
				case "System.Single":
				case "System.Decimal":
				case "System.Double":
					return true;
				default:
					return false;
			}
		}

		private static bool IsArray(TypeReference aType) {
			TypeReference xCurType = aType;
			do {
				if (xCurType.FullName == "System.Array") {
					return true;
				}
				xCurType = Engine.GetDefinitionFromTypeReference(xCurType).BaseType;
			} while (xCurType != null);
			return false;
		}

		public Ldsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			TypeDefinition xFieldTypeDef = Engine.GetDefinitionFromTypeReference(xField.FieldType);
			mIsReferenceTypeOrStruct = (xFieldTypeDef.IsClass || xFieldTypeDef.IsValueType || IsArray(xFieldTypeDef)) && !IsPrimitive(xFieldTypeDef);
			Engine.QueueStaticField(xField, out mDataName);
			mSize = Engine.GetFieldStorageSize(xFieldTypeDef);
			TypeSpecification xTypeSpec = xField.FieldType as TypeSpecification;
			if(xTypeSpec != null) {
				mIsReferenceTypeOrStruct = true;
			}
			if (String.IsNullOrEmpty(mDataName)) {
				throw new Exception("No name generated for field '" + xField.GetFullName() + "'");
			}
			//DoQueueStaticField(xField.DeclaringType.Module.Assembly.Name.FullName, xField.DeclaringType.FullName, xField.Name, out mDataName);
		}
		public override void DoAssemble() {
			Literal("; Is ReferenceTypeOrStruct = " + mIsReferenceTypeOrStruct.ToString());
			if (!mIsReferenceTypeOrStruct) {
				Pushd("[" + mDataName + "]");
			} else {
				Pushd("[" + mDataName + "]");
			}
			Assembler.StackSizes.Push(mSize);
		}
	}
}