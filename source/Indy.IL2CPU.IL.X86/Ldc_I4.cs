using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldc_I4)]
	public class Ldc_I4: Op {
		private int mValue;
		private bool mOptimizedArrayFieldCode = false;
		private FieldDefinition mTokenField;
		private FieldDefinition mStaticField;
		private int mCount;
		protected void SetValue(int aValue) {
			mValue = aValue;
		}

		protected void SetValue(string aValue) {
			SetValue(Int32.Parse(aValue));
		}

		public Ldc_I4(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			// see if this opcode is part of a situation which can be optimized
			#region explanation
			/*
			 * See the following MSIL snippet as example:
			 * 
			 *		L_000a: ldc.i4.s 0x19
			 *		L_000c: newarr char
			 *		L_0011: dup 
    		 *		L_0012: ldtoken valuetype <PrivateImplementationDetails>{0902461D-F9D1-470E-8BBD-C644516033E3}/__StaticArrayInitTypeSize=50 <PrivateImplementationDetails>{0902461D-F9D1-470E-8BBD-C644516033E3}::$$method0x60052b0-1
    		 *		L_0017: call void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class System.Array, valuetype System.RuntimeFieldHandle)
    		 *		L_001c: stsfld char[] System.String::WhitespaceChars
			 *		
			 * Practically speaking, we can optimize this code (completey get rid of it) by 
			 * initializing the field WhitespaceChars directly using the contents of the token
			 * 
			 */
			if (aInstruction.Next != null && aInstruction.Next.OpCode.Code == Code.Newarr) {
				Instruction xNewArr = aInstruction.Next;
				if (xNewArr.Next != null && xNewArr.Next.OpCode.Code == Code.Dup) {
					Instruction xDup = xNewArr.Next;
					if (xDup.Next != null && xDup.Next.OpCode.Code == Code.Ldtoken) {
						Instruction xLdtoken = xDup.Next;
						if (xLdtoken.Next != null && xLdtoken.Next.OpCode.Code == Code.Call) {
							Instruction xCall = xLdtoken.Next;
							if (xCall.Next != null && xCall.Next.OpCode.Code == Code.Stsfld) {
								Instruction xStsfld = xCall.Next;
								// so far so good. only next constraint is that Call calls RuntimeHelpers::InitializeArray
								// and the field to which it's stored is readonly
								MethodReference xMethodRef = xCall.Operand as MethodReference;
								FieldReference xFieldRef = xStsfld.Operand as FieldReference;
								FieldReference xFieldContentsRef = xLdtoken.Operand as FieldReference;
								if (xFieldRef != null && xMethodRef != null) {
									MethodDefinition xMethodDef = Engine.GetDefinitionFromMethodReference(xMethodRef);
									FieldDefinition xFieldDef = Engine.GetDefinitionFromFieldReference(xFieldRef);
									FieldDefinition xFieldContentsDef = Engine.GetDefinitionFromFieldReference(xFieldContentsRef);
									if (xFieldDef.IsInitOnly && xMethodDef.ToString() == "System.Void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array,System.RuntimeFieldHandle)") {
										mTokenField = xFieldContentsDef;
										mStaticField = xFieldDef;
										mOptimizedArrayFieldCode = true;
										mCount = Int32.Parse(aInstruction.Operand.ToString());
										Engine.SetInstructionsToSkip(5);
									}
								}
							}
						}
					}
				}
			}
			#endregion
			if (!mOptimizedArrayFieldCode) {
				if (aInstruction.Operand != null) {
					SetValue(aInstruction.Operand.ToString());
				}
			}
		}

		public int Value {
			get {
				return mValue;
			}
		}
		public override sealed void DoAssemble() {
			if (mOptimizedArrayFieldCode) {
				new Comment("Optimization:");
				string xDataMemberName = DataMember.GetStaticFieldName(mStaticField);
				DataMember xFieldDataMember = (from item in Assembler.DataMembers
											   where item.Name == xDataMemberName
											   select item).FirstOrDefault();
				Assembler.DataMembers.Remove(xFieldDataMember);
				xFieldDataMember = null;
				string xTheData = BitConverter.GetBytes(Engine.RegisterTypeRef(mStaticField.FieldType)).Aggregate("", (r, b) => r + b + ",");
				xTheData += BitConverter.GetBytes(0x80000002).Aggregate("", (r, b) => r + b + ",");
				xTheData += BitConverter.GetBytes(mCount).Aggregate("", (r, b) => r + b + ",");
				xTheData += mTokenField.InitialValue.Aggregate("", (r, b) => r + b + ",");
				xTheData = xTheData.TrimEnd(',');
				Assembler.DataMembers.Add(new DataMember(xDataMemberName, "db", xTheData));

			} else {
				new CPU.Pushd("0" + mValue.ToString("X") + "h");
				Assembler.StackSizes.Push(4);
			}
		}
	}
}