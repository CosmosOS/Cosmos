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
			if(aInstruction.ToString() == "Ldsfld System.Int32 Indy.IL2CPU.IL.X86.Native.CustomImplementations.System.ConsoleImpl::mCurrentLine") {
				System.Diagnostics.Debugger.Break();
			}
			mIsReference = Engine.GetDefinitionFromTypeReference(xField.FieldType).IsClass;
			DoQueueStaticField(xField.DeclaringType.Module.Assembly.Name.FullName, xField.DeclaringType.FullName, xField.Name, out mDataName);
		}
		public override void DoAssemble() {
//			if(mIsReference && Assembler.InMetalMode) {
//				Pushd(4, mDataName);
//				return;
//			}
			Literal("; Size = " + mSize + ", IsReference = " + mIsReference);
			for (int i = 0; i < (mSize / 4); i++) {
			//	Pop("eax");
			//	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
				Assembler.Add(new CPU.Pushd("[" + mDataName + " + 0x" + (i * 4).ToString("X") + "]"));
			}
			switch (mSize % 4) {
				case 1: {
						Assembler.Add(new CPU.Push("byte [" + mDataName + " + 0x" + (mSize - 1).ToString("X") + "]"));
						break;
					}
				case 2: {
						Assembler.Add(new CPU.Push("word [" + mDataName + " + 0x" + (mSize - 2).ToString("X") + "]"));
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (mSize % 4) + " not supported!");

			}
			Assembler.StackSizes.Push(mSize);
		}
	}
}