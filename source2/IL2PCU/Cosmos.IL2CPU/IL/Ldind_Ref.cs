using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldind_Ref)]
	public class Ldind_Ref: ILOpCode
	{



		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldind_Ref)]
		// 	public class Ldind_Ref: ILOpCode {
		// 		public Ldind_Ref(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		//             new CPU.Pop { DestinationReg = CPU.Registers.EAX };
		//             new CPU.Push { DestinationReg = CPU.Registers.EAX, DestinationIsIndirect = true };
		// 		}
		// 	}
		// }
		#endregion
	}
}
