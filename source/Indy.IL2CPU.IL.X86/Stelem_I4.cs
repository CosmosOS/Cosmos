using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stelem_I4)]
	public class Stelem_I4: Op {
		public Stelem_I4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		public override void DoAssemble() {
			Stelem_Ref.Assemble(Assembler, 4);
		}
	}
}