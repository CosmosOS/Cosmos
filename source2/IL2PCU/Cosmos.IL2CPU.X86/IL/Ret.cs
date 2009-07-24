using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ret)]
	public class Ret: ILOpX86
	{
		public Ret(ILOpCode aOpCode):base(aOpCode)
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
		// 	[OpCode(OpCodeEnum.Ret)]
		// 	public class Ret: Op {
		// 		public Ret(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		//             new CPU.Jump { DestinationLabel = MethodFooterOp.EndOfMethodLabelNameNormal };
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
