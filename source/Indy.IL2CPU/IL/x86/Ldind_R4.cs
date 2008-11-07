using System;

using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldind_R4)]
	public class Ldind_R4: Op {
		public Ldind_R4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			throw new Exception("Floats not supported yet");
			//Assembler.StackContents.Pop();
			//new CPUx86.Pop(CPUx86.Registers_Old.EAX);
			//new CPUx86.Pushd(CPUx86.Registers_Old.EAX);
			//Assembler.StackContents.Push(4);
		}
	}
}