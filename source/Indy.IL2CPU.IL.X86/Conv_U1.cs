using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

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
						Pop("eax");
						Pushd("eax");
						Assembler.StackSizes.Push(1);
						break;
					}
				case 8: {
						Pop("eax");
						Pop("ecx");
						Pushd("eax");
						Assembler.StackSizes.Push(1);
						break;
					}
				case 1: {
					Assembler.StackSizes.Push(1);
					break;
				}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
		}
	}
}