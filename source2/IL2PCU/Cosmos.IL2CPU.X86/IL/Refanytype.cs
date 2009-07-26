using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Refanytype)]
	public class Refanytype: ILOp
	{
		public Refanytype(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Refanytype)]
		// 	public class Refanytype: Op {
		// 		public Refanytype(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// //			Pop("eax");
		// //			Pushd("[eax]");
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
