using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelem_I8, true)]
	public class Ldelem_I8: Op {
		public Ldelem_I8(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		
		public override void DoAssemble() {
			Ldelem_Ref.Assemble(Assembler, 8);
		}
	}
}