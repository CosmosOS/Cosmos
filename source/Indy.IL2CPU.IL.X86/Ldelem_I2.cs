using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelem_I2, true)]
	public class Ldelem_I2: Op {
		public Ldelem_I2(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			Ldelem_Ref.Assemble(Assembler, 2);
		}
	}
}