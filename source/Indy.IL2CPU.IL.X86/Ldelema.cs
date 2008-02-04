using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelema, true)]
	public class Ldelema: Op {
		private int mElementSize;
		public Ldelema(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xTypeRef = aReader.OperandValueType;
			mElementSize = Engine.GetFieldStorageSize(xTypeRef);
		}

		public static void Assemble(CPU.Assembler aAssembler, int aElementSize) {
			aAssembler.StackContents.Pop();
			aAssembler.StackContents.Pop();
			aAssembler.StackContents.Push(new StackContent(4, typeof(uint)));
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Move(CPUx86.Registers.EDX, "0" + aElementSize.ToString("X") + "h");
			new CPUx86.Multiply(CPUx86.Registers.EDX);
			new CPUx86.Add(CPUx86.Registers.EAX, "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h");
			new CPUx86.Pop(CPUx86.Registers.EDX);
			new CPUx86.Add(CPUx86.Registers.EDX, CPUx86.Registers.EAX);
			new CPUx86.Pushd(CPUx86.Registers.EDX);
		}

		public override void DoAssemble() {
			Assemble(Assembler, mElementSize);
		}
	}
}