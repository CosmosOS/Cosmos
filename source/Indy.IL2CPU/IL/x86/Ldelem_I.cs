using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelem_I)]
	public class Ldelem_I: Op {
		public Ldelem_I(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			// todo: add support for different pointer sizes
			Ldelem_Ref.Assemble(Assembler, 4);
		}
	}
}