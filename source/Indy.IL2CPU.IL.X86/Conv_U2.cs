using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_U2)]
	public class Conv_U2: Op {
		public Conv_U2(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSource = Assembler.StackSizes.Pop();
			switch (xSource) {
				case 1:
				case 4: {
						Pop("eax");
						Pushd("eax");
						Assembler.StackSizes.Push(2);
						break;
					}
				case 8: {
						Pop("eax");
						Pop("ecx");
						Pushd("eax");
						Assembler.StackSizes.Push(2);
						break;
					}
				case 2: {
						Assembler.StackSizes.Push(2);
						break;
					}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
		}
	}
}