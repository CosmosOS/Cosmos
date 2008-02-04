using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelem_U4, true)]
	public class Ldelem_U4: Op {
		public Ldelem_U4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		
		public override void DoAssemble() {
			Ldelem_Ref.Assemble(Assembler, 4);
		}
	}
}