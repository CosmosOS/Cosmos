using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Add_Ovf_Un)]
	public class Add_Ovf_Un: Add_Ovf {
		public Add_Ovf_Un(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		public override void DoAssemble()
		{
			AddWithOverflow(Assembler, false);
		}
	}
}