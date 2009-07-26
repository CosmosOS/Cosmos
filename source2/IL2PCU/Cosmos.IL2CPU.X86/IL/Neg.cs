using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Neg)]
	public class Neg: ILOp
	{
		public Neg(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      throw new Exception("TODO:");
    }

    #region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Neg)]
		// 	public class Neg: Op {
		// 		public Neg(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
		// 			new CPUx86.Neg{DestinationReg=CPUx86.Registers.EAX};
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
