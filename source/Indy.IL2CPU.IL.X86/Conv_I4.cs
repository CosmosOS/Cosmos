using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_I4)]
	public class Conv_I4: Op {
		public Conv_I4(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSource = Assembler.StackSizes.Pop();
			switch (xSource) {
				case 1:
				case 2:
				case 4: {
						break;
					}
				case 8: {
						Assembler.StackSizes.Peek();
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Add("esp", "4");
						new CPUx86.Pushd(CPUx86.Registers.EAX);
						break;

					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackSizes.Push(4);
		}
	}
}