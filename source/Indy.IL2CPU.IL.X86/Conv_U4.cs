using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_U4)]
	public class Conv_U4: Op {
		public Conv_U4(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			// todo: WARNING: not implemented correctly!
			int xSource = Assembler.StackSizes.Peek();
			switch (xSource) {
				case 1:
				case 2: {
						break;
					}
				case 8: {
					Pop("eax");
					Pop("ecx");
					Pushd(4, "eax");
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