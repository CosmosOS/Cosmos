using System;
using System.Collections.Generic;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Initobj)]
	public class Initobj: Op {
		private uint mObjSize;

        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            Type xTypeRef = aReader.OperandValueType;
            if (xTypeRef == null)
            {
                throw new Exception("Type not found!");
            }
            Engine.RegisterType(xTypeRef);
        }

		public Initobj(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xTypeRef = aReader.OperandValueType;
			if (xTypeRef == null) {
				throw new Exception("Type not found!");
			}
			mObjSize = 0;
			if (xTypeRef.IsValueType) {
				Engine.GetTypeFieldInfo(xTypeRef, out mObjSize);
			}
		}

		public override void DoAssemble() {
			Assembler.StackContents.Pop();
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
			for (int i = 0; i < (mObjSize / 4); i++) {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceValue = 0, Size=32 };
			}
			switch (mObjSize % 4) {
				case 1: {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)((mObjSize / 4) * 4), SourceValue = 0, Size = 8 };
                        break;
					}
				case 2: {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)((mObjSize / 4) * 4), SourceValue = 0, Size = 16 };
                        break;
					}
				case 0:
					break;
				default: {
						throw new Exception("Remainder size " + mObjSize % 4 + " not supported yet!");
					}
			}
		}
	}
}