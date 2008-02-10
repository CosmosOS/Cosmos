using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldobj)]
	public class Ldobj: Op {
		private Type xType;
		public Ldobj(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			xType = aReader.OperandValueType;
			if (xType == null) {
				throw new Exception("Type specification not found!");
			}
			mSize = Engine.GetFieldStorageSize(xType);
		}

		private int mSize;
		public override void DoAssemble() {
			new CPUx86.Call("_CODE_REQUESTED_BREAK_");
			new CPUx86.Pop(CPUx86.Registers.EAX);
			for (int i = 1; i <= (mSize / 4); i++) {
				new CPUx86.Pushd("[eax + " + (mSize - (i * 4)) + "]");
			}
			switch (mSize % 4) {
				case 1: {
						new CPUx86.Push("byte", CPUx86.Registers.AtEAX);
						break;
					}
				case 2: {
						new CPUx86.Push("word", CPUx86.Registers.AtEAX);
						break;
					}
				case 0: {
						break;
					}
				default: {
						throw new Exception("Remainder not supported!");
					}
			}
			Assembler.StackContents.Pop();
			Assembler.StackContents.Push(new StackContent(mSize, xType));
		}
	}
}