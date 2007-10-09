using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_U)]
	public class Conv_U: Op {
		public Conv_U(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSource = Assembler.StackSizes.Pop();
			switch (xSource) {
				case 1:
				case 2: {
						Pop("eax");
						Pushd("eax");
						Assembler.StackSizes.Push(4);
						break;
					}
				case 8: {
						Pop("eax");
						Pop("ecx");
						Pushd("eax");
						Assembler.StackSizes.Push(4);
						break;
					}
				case 4: {
						Assembler.StackSizes.Push(4);
						break;
					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
		}
	}
}