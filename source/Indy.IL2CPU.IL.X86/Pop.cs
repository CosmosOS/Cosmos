using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Pop)]
	public class Pop: Op {
		private bool mNeeded = true;
		public Pop(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aInstruction.Previous != null &&
				(aInstruction.Previous.OpCode.Code == Code.Call ||
				aInstruction.Previous.OpCode.Code == Code.Calli ||
				aInstruction.Previous.OpCode.Code == Code.Callvirt)) {
				mNeeded = false;
			}
		}
		public override void Assemble() {
			if (mNeeded) {
				Pop("eax");
			}
		}
	}
}