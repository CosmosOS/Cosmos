using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Pop)]
	public class Pop: ILOp
	{
		public Pop(ILOpCode aOpCode):base(aOpCode)
		{
		}

    public override void Execute(uint aMethodUID) {
      throw new Exception("TODO:");
    }

    #region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Pop)]
		// 	public class Pop: Op {
		// 		public Pop(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 
		// 		}
		// 		public override void DoAssemble() {
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		// 			Assembler.StackContents.Pop();
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
