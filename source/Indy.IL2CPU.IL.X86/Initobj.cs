using System;


using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Initobj)]
	public class Initobj: Op {
		private int mObjSize;
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
			new CPUx86.Pop(CPUx86.Registers.EAX);
			for (int i = 0; i < (mObjSize / 4); i++) {
				new CPUx86.Move("dword [eax + 0" + (i * 4).ToString("X") + "h]", "0");
			}
			switch (mObjSize % 4) {
				case 1: {
						new CPUx86.Move("byte [eax + 0" + ((mObjSize / 4) * 4).ToString("X") + "h]", "0");
						break;
					}
				case 2: {
						new CPUx86.Move("word [eax + 0" + ((mObjSize / 4) * 4).ToString("X") + "h]", "0");
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