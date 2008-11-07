using System;
using System.Collections.Generic;
using System.Linq;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Box)]
	public class Box: Op {
		private uint mTheSize;
		private int mTypeId;

        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            Type xTypeRef = aReader.OperandValueType as Type;
            if (xTypeRef == null)
            {
                throw new Exception("Couldn't determine Type!");
            } 
            Engine.RegisterType(xTypeRef);
            Engine.QueueMethod(GCImplementationRefs.AllocNewObjectRef);
        }

	    public Box(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xTypeRef = aReader.OperandValueType as Type;
			if (xTypeRef == null) {
				throw new Exception("Couldn't determine Type!");
			}
			//if (xTypeRef is GenericParameter) {
			//    // todo: implement support for generics
			//    mTheSize = 4;
			//} else {
				mTheSize = Engine.GetFieldStorageSize(xTypeRef);
			//}
			//if (((mTheSize / 4) * 4) != mTheSize) {
			//    throw new Exception("Incorrect Datasize. ( ((mTheSize / 4) * 4) === mTheSize should evaluate to true!");
			//}
			//if (!(xTypeRef is GenericParameter)) {
                mTypeId = Engine.RegisterType(xTypeRef);
			//}
		}

		public override void DoAssemble() {
			uint xSize = mTheSize;
			if (mTheSize % 4 != 0) {
				xSize += 4 - (mTheSize % 4);
			}
            new CPUx86.Push { DestinationValue = (ObjectImpl.FieldDataOffset + xSize) };
            new CPUx86.Call { DestinationLabel = CPU.Label.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef) };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; 
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceValue = (uint)mTypeId, Size=32 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.BoxedValueType, Size=32 };
            new CPU.Comment("xSize is " + xSize);
            for (int i = 0; i < (xSize / 4); i++)
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (ObjectImpl.FieldDataOffset + (i * 4)), SourceReg = CPUx86.Registers.EDX, Size=32 };
            }
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            Assembler.StackContents.Pop();
            Assembler.StackContents.Push(new StackContent(4, false, false, false));
		} 
	}
}