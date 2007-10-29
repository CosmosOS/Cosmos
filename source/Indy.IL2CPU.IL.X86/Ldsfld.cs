using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsfld)]
	public class Ldsfld: Op {
		private string mDataName;
		private int mSize;
		private bool mIsReference = false;

		public Ldsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			mSize = Engine.GetFieldStorageSize(xField.FieldType);
			mIsReference = Engine.GetDefinitionFromTypeReference(xField.FieldType).IsClass;
			Engine.QueueStaticField(Engine.GetDefinitionFromFieldReference( xField));
		}
		public override void DoAssemble() {
			//			if(mIsReference && Assembler.InMetalMode) {
			//				Pushd(4, mDataName);
			//				return;
			//			}
			if (mSize > 4) {
				for (int i = 0; i < (mSize / 4); i++) {
					//	Pop("eax");
					//	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
					Assembler.Add(new CPU.Pushd("[" + mDataName + " + 0x" + (i * 4).ToString("X") + "]"));
				}
				switch (mSize % 4) {
					case 1: {
							Assembler.Add(new CPU.Move("eax", "0"));
							Assembler.Add(new CPU.Move("al", "[" + mDataName + " + 0x" + (mSize - 1).ToString("X") + "]"));
							Assembler.Add(new CPU.Push("eax"));
							break;
						}
					case 2: {
							Assembler.Add(new CPU.Move("eax", "0"));
							Assembler.Add(new CPU.Move("ax", "[" + mDataName + " + 0x" + (mSize - 1).ToString("X") + "]"));
							Assembler.Add(new CPU.Push("eax"));
							break;
						}
					case 0: {
							break;
						}
					default:
						throw new Exception("Remainder size " + (mSize % 4) + " not supported!");
				}
			} else {
				switch (mSize % 4) {
					//					case 1: {
					//							Assembler.Add(new CPU.Push("byte " + mDataName));
					//							break;
					//						}
					//					case 2: {
					//							Assembler.Add(new CPU.Push("word " + mDataName));
					//							break;
					//						}
					case 1:
					case 2:
					case 4: {
							Assembler.Add(new CPU.Push(mDataName));
							break;
						}
					case 0: {
							break;
						}
					default:
						throw new Exception("Remainder size " + (mSize % 4) + " not supported!");
				}
			}
			Assembler.StackSizes.Push(mSize);
		}
	}
}