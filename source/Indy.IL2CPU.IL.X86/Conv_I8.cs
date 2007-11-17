using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_I8)]
	public class Conv_I8: Op {
		public Conv_I8(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSource = Assembler.StackSizes.Pop();
			switch(xSource) {
				case 1:
				case 2:
				case 4: {
					new CPU.Pop("eax");
					new CPU.Pushd("0");
					new CPU.Pushd("eax");
					Assembler.StackSizes.Push(8);
					break;
				}
				case 8: {
					break;
				}
				default:
					throw new Exception("SourceSize " + xSource + " not supported!");
			}
		}
	}
}