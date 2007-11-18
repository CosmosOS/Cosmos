using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_U1)]
	public class Conv_U1: Op {
		public Conv_U1(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSource = Assembler.StackSizes.Pop();
			switch (xSource) {
				case 2:
				case 4: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Pushd(CPUx86.Registers.EAX);
						break;
					}
				case 8: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Pop(CPUx86.Registers.ECX);
						new CPUx86.Pushd(CPUx86.Registers.EAX);
						break;
					}
				case 1: {
						break;
					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackSizes.Push(1);
		}
	}
}