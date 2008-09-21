using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Shl)]
	public class Shl: Op {
		public Shl(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.EAX); // shift amount
			var xStackItem_ShiftAmount = Assembler.StackContents.Pop();
			new CPUx86.Pop(CPUx86.Registers.EDX); // value
			var xStackItem_Value = Assembler.StackContents.Pop();
			new CPUx86.Move(CPUx86.Registers.EBX, "0");
			new CPUx86.Move(CPUx86.Registers.CL, CPUx86.Registers.AL);
			new CPUx86.ShiftLeft(CPUx86.Registers.EDX, CPUx86.Registers.EBX, CPUx86.Registers.CL);
			new CPUx86.Pushd(CPUx86.Registers.EDX);
			Assembler.StackContents.Push(xStackItem_Value);
		}
	}
}