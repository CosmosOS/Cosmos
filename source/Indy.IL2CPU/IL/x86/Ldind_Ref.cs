using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldind_Ref)]
	public class Ldind_Ref: Op {
		public Ldind_Ref(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
            new CPU.Pop { DestinationReg = CPU.Registers.EAX };
            new CPU.Push { DestinationReg = CPU.Registers.EAX, DestinationIsIndirect = true };
		}
	}
}