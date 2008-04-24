using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Mul_Ovf)]
	public class Mul_Ovf: Mul {
		public Mul_Ovf(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		public override void DoAssemble()
		{
			throw new NotImplementedException();
			//base.DoAssemble();
		}
	}
}