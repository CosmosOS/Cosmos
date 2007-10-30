using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Rem)]
	public class Rem: Op {
		public Rem(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Assembler.StackSizes.Peek();
			Pop("ecx");
			Pop("eax"); // gets devised by ecx
			Xor("edx", "edx");
			Assembler.Add(new CPU.Divide("ecx"));
			Pushd(xSize, "edx");

	
//			this.MovRegisterOperand(R32.EAX, first);
//			this.MovRegisterOperand(R32.ECX, second);
//			this.assembly.XOR(R32.EDX, R32.EDX);
//
//			assembly.DIV(R32.ECX);
//
//			assembly.MOV(register, R32.EDX);

		}
	}
}