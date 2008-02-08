using System;
using Indy.IL2CPU.Assembler;


using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldsfld)]
	public class Ldsfld: Op {
		private string mDataName;
		private int mSize;
		private bool mNeedsGC;
		private Type mDataType;

		public Ldsfld(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			FieldInfo xField = aReader.OperandValueField;
			mSize = Engine.GetFieldStorageSize(xField.FieldType);
			Engine.QueueStaticField(xField, out mDataName);
			mNeedsGC = !xField.FieldType.IsValueType && xField.FieldType.FullName != "System.String";
		}
		public override void DoAssemble() {
			if (mSize >= 4) {
				for (int i = 1; i <= (mSize / 4); i++) {
					//	Pop("eax");
					//	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
					new CPUx86.Pushd("[" + mDataName + " + 0x" + (mSize - (i * 4)).ToString("X") + "]");
				}
				switch (mSize % 4) {
					case 1: {
							new CPUx86.Move(CPUx86.Registers.EAX, "0");						//mSize - 1
							new CPUx86.Move(CPUx86.Registers.AL, "[" + mDataName + " + 0x" + (0).ToString("X") + "]");
							new CPUx86.Push(CPUx86.Registers.EAX);
							break;
						}
					case 2: {
							new CPUx86.Move(CPUx86.Registers.EAX, "0");
							new CPUx86.Move(CPUx86.Registers.AX, "[" + mDataName + " + 0x" + (0).ToString("X") + "]");
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
			Assembler.StackContents.Push(new StackContent(mSize, mDataType));
			if (mNeedsGC) {
				new Dup(null, null) {
					Assembler = this.Assembler
				}.
				Assemble();
				Engine.QueueMethod(GCImplementationRefs.IncRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
				Assembler.StackContents.Pop();
			}
		}
	}
}