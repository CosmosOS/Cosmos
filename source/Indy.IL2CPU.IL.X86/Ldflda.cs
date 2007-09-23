using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldflda)]
	public class Ldflda: Op {
		private readonly string mRelativeAddress;
		public Ldflda(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			
		}

		public Ldflda(string aRelativeAddress):base(null, null) {
			mRelativeAddress = aRelativeAddress;
		}

		public override void DoAssemble() {
			Pop("eax");
			Assembler.Add(new CPU.Add("eax", mRelativeAddress.Trim().Substring(1)));
			Pushd("eax");
		}
	}
}