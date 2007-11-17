using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Conv_U8)]
	public class Conv_U8: Op {
		public Conv_U8(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSource = Assembler.StackSizes.Peek();
			switch (xSource) {
				case 1:
				case 2:
				case 4: {
						Assembler.StackSizes.Pop();
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