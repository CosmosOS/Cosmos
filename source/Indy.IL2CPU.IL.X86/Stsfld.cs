using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stsfld)]
	public class Stsfld: Op {
		private string mDataName;
		private int mSize;

		public Stsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			mSize = Engine.GetFieldStorageSize(xField.FieldType);
			Engine.QueueStaticField(xField, out mDataName);
		}

		public override void DoAssemble() {
			//if (mIsReference && Assembler.InMetalMode) {
			//	Pushd(4, "[" mDataName);
			//	return;
			//}
			for (int i = 1; i <= (mSize / 4); i++) {
				Pop("eax");
				Move(Assembler, "dword [" + mDataName + " + 0x" + (mSize - (i * 4)).ToString("X") + "]", "eax");
			}
			switch (mSize % 4) {
				case 1: {
						Pop("eax");
						Move(Assembler, "byte [" + mDataName + "]", "al");
						break;
					}
				case 2: {
						Pop("eax");
						Move(Assembler, "word [" + mDataName + "]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (mSize % 4) + " not supported!");

			}
			Assembler.StackSizes.Pop();
		}
	}
}