using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldind_I4)]
	public class Ldind_I4: ILOp
	{
		public Ldind_I4(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
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
		// 	[OpCode(OpCodeEnum.Ldind_I4)]
		// 	public class Ldind_I4: Op {
		// 		public Ldind_I4(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		// 		    new CPUx86.Push {
		// 		                            DestinationReg = CPUx86.Registers.EAX,
		// 		                            DestinationIsIndirect = true
		// 		                    };
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
