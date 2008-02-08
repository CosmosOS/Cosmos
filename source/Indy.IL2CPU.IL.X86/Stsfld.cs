using System;
using Indy.IL2CPU.Assembler;


using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stsfld)]
	public class Stsfld: Op {
		private string mDataName;
		private int mSize;
		private Type mDataType;
		private bool mNeedsGC;
		private string mBaseLabel;


		public Stsfld(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			FieldInfo xField = aReader.OperandValueField;
			mSize = Engine.GetFieldStorageSize(xField.FieldType);
			Engine.QueueStaticField(xField, out mDataName);
			mNeedsGC = !xField.FieldType.IsValueType && xField.FieldType.FullName != "System.String";
			mDataType = xField.FieldType;
			mBaseLabel = GetInstructionLabel(aReader);
		}

		public override void DoAssemble() {
			if (mNeedsGC) {
				new CPUx86.Pushd("[" + mDataName + "]");
				Engine.QueueMethod(GCImplementationRefs.DecRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
			}
			for (int i = 0; i < (mSize / 4); i++) {
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Move("dword [" + mDataName + " + 0x" + ((i * 4)).ToString("X") + "]", "eax");
			}
			switch (mSize % 4) {
				case 1: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Move("byte [" + mDataName + " + " + ((mSize/4)*4)  + "]", "al");
						break;
					}
				case 2: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Move("word [" + mDataName + " + " + ((mSize / 4) * 4) + "]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (mSize % 4) + " not supported!");

			}
			Assembler.StackContents.Pop();
		}
	}
}