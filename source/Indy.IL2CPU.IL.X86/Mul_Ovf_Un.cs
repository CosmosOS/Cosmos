using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Mul_Ovf_Un)]
	public class Mul_Ovf_Un: Mul_Ovf {
		public Mul_Ovf_Un(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
	}
}