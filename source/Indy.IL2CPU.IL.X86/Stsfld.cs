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
			DoQueueStaticField(xField.DeclaringType.Module.Assembly.Name.FullName, xField.DeclaringType.FullName, xField.Name, out mDataName);
		}

		public override void DoAssemble() {
			for (int i = 0; i < (mSize / 4); i++) {
				Pop("eax");
				Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
			}
			switch (mSize % 4) {
				case 1: {
						Pop("al");
						Move(Assembler, "byte [" + mDataName + " + 0x" + (mSize - 1).ToString("X") + "]", "al");
						break;
					}
				case 2: {
						Pop("ax");
						Move(Assembler, "word [" + mDataName + " + 0x" + (mSize - 2).ToString("X") + "]", "al");
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