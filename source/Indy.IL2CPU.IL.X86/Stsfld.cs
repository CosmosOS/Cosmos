using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

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
			for (int i = 1; i <= (mSize / 4); i++) {
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Move("dword [" + mDataName + " + 0x" + (mSize - (i * 4)).ToString("X") + "]", "eax");
			}
			switch (mSize % 4) {
				case 1: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Move("byte [" + mDataName + "]", "al");
						break;
					}
				case 2: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Move("word [" + mDataName + "]", "ax");
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