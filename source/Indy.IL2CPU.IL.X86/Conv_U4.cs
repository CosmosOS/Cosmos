using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_U4)]
	public class Conv_U4: Op {
		public Conv_U4(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			// todo: WARNING: not implemented correctly!
			int xSource = Assembler.StackSizes.Pop();
			switch (xSource) {
				case 1:
				case 2: {
						break;
					}
				case 8: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Pop(CPUx86.Registers.ECX);
						new CPUx86.Pushd(CPUx86.Registers.EAX);
						Assembler.StackSizes.Push(4);
						break;
					}
				case 4: {
						break;
					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
		}
	}
}