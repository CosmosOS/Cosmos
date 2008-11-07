using System;
using System.Collections.Generic;
using Indy.IL2CPU.Assembler;


using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stsfld)]
	public class Stsfld: Op {
		private string mDataName;
		private uint mSize;
		private Type mDataType;
		private bool mNeedsGC;
		private string mBaseLabel;

        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            FieldInfo xField = aReader.OperandValueField;
            Engine.QueueStaticField(xField);
        }

		public Stsfld(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			FieldInfo xField = aReader.OperandValueField;
			mSize = Engine.GetFieldStorageSize(xField.FieldType);
			Engine.QueueStaticField(xField, out mDataName);
			mNeedsGC = !xField.FieldType.IsValueType;
			mDataType = xField.FieldType;
			mBaseLabel = GetInstructionLabel(aReader);
		}

		public override void DoAssemble() {
			if (mNeedsGC) {
                new CPUx86.Push { DestinationRef = new ElementReference(mDataName), DestinationIsIndirect = true };
				Engine.QueueMethod(GCImplementationRefs.DecRefCountRef);
                new CPUx86.Call { DestinationLabel = Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
			}
			for (int i = 0; i < (mSize / 4); i++) {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationRef = new ElementReference(mDataName, i * 4), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
			}
			switch (mSize % 4) {
				case 1: {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Move { DestinationRef = new ElementReference(mDataName, (int)((mSize / 4) * 4)), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.AL };
						break;
					}
				case 2: {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Move { DestinationRef = new ElementReference(mDataName, (int)((mSize / 4) * 4)), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.AX };
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