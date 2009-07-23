using System;

namespace Cosmos.IL2CPU.ILOpCodes
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Br)]
	public class Br: ILOpCode
	{



		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Br)]
		// 	public class Br: ILOpCode {
		// 		private readonly string mTargetInstructionName;
		// 		public Br(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mTargetInstructionName = GetInstructionLabel(aReader.OperandValueBranchPosition);
		// 		}
		// 		public override void DoAssemble() {
		//             new CPU.Jump { DestinationLabel = mTargetInstructionName };
		// 		}
		// 	}
		// }
		#endregion
	}
}
