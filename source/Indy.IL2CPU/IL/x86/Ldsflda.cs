using System;
using System.Collections.Generic;
using CPU = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldsflda)]
	public class Ldsflda: Op {
		private readonly string mDataName;
        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            FieldInfo xField = aReader.OperandValueField;
            Engine.QueueStaticField(xField);
        }

		public Ldsflda(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			FieldInfo xField = aReader.OperandValueField;
			Engine.QueueStaticField(xField, out mDataName);
		}

		public override void DoAssemble() {
            new CPU.Push { DestinationRef = new ElementReference(mDataName) };
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		}
	}
}