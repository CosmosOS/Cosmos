using System;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsfld)]
	public class Ldsfld: Op {
		private string mDataName;
		private int mSize;
		private bool mNeedsGC;

		public Ldsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			mSize = Engine.GetFieldStorageSize(xField.FieldType);
			Engine.QueueStaticField(Engine.GetDefinitionFromFieldReference(xField), out mDataName);
			mNeedsGC = !Engine.GetDefinitionFromTypeReference(xField.FieldType).IsValueType && xField.FieldType.FullName != "System.String";
		}
		public override void DoAssemble() {
			if (mSize >= 4) {
				for (int i = 0; i < (mSize / 4); i++) {
					//	Pop("eax");
					//	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
					new CPUx86.Pushd("[" + mDataName + " + 0x" + (i * 4).ToString("X") + "]");
				}
				switch (mSize % 4) {
					case 1: {
							new CPUx86.Move(CPUx86.Registers.EAX, "0");
							new CPUx86.Move(CPUx86.Registers.AL, "[" + mDataName + " + 0x" + (mSize - 1).ToString("X") + "]");
							new CPUx86.Push(CPUx86.Registers.EAX);
							break;
						}
					case 2: {
							new CPUx86.Move(CPUx86.Registers.EAX, "0");
							new CPUx86.Move(CPUx86.Registers.AX, "[" + mDataName + " + 0x" + (mSize - 2).ToString("X") + "]");
							new CPUx86.Push(CPUx86.Registers.EAX);
							break;
						}
					case 0: {
							break;
						}
					default:
						throw new Exception("Remainder size " + (mSize % 4) + " not supported!");
				}
			} else {
				switch (mSize) {
					case 1: {
							new CPUx86.Move(CPUx86.Registers.EAX, "0");
							new CPUx86.Move(CPUx86.Registers.AL, "[" + mDataName + "]");
							new CPUx86.Push(CPUx86.Registers.EAX);
							break;
						}
					case 2: {
							new CPUx86.Move(CPUx86.Registers.EAX, "0");
							new CPUx86.Move(CPUx86.Registers.AX, "[" + mDataName + "]");
							new CPUx86.Push(CPUx86.Registers.EAX);
							break;
						}
					case 0: {
							break;
						}
					default:
						throw new Exception("Remainder size " + (mSize) + " not supported!");
				}
			}
			Assembler.StackSizes.Push(mSize);
			if (mNeedsGC) {
				new Dup(null, null) {
					Assembler = this.Assembler
				}.
				Assemble();
				Engine.QueueMethodRef(GCImplementationRefs.IncRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
				Assembler.StackSizes.Pop();
			}
		}
	}
}