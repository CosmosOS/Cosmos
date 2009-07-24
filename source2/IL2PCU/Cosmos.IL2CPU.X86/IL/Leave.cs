using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Leave)]
	public class Leave: ILOpX86
	{
		public Leave(ILOpCode aOpCode):base(aOpCode)
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
		// 	[OpCode(OpCodeEnum.Leave)]
		// 	public class Leave: Op {public readonly string TargetLabel;
		// 	public Leave(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		// 		}
		// 		public override void DoAssemble() {
		//         new CPU.Jump { DestinationLabel = TargetLabel };
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
