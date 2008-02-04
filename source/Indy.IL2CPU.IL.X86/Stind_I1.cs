using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stind_I1)]
	public class Stind_I1: Op {
		public Stind_I1(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			Stind_I.Assemble(Assembler, 1);
		}
	}
}