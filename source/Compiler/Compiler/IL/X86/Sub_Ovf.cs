using System;

namespace Cosmos.Compiler.IL.X86
{
	[OpCode(OpCodeEnum.Sub_Ovf)]
	public class Sub_Ovf: Op
	{



		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Sub_Ovf)]
		// 	public class Sub_Ovf: Op {
		// 		private string mCurrentLabel;
		// 	    private string mNextLabel;
		// 	    private uint mCurrentOffset;
		// 	    private MethodInformation mCurrentMethodInfo;
		//         public Sub_Ovf(ILReader aReader, MethodInformation aMethodInfo)
		//             : base(aReader, aMethodInfo)
		//         {
		//             mCurrentLabel = GetInstructionLabel(aReader);
		//             mCurrentOffset = aReader.Position;
		//             mNextLabel = GetInstructionLabel(aReader.NextPosition);
		//             mCurrentMethodInfo = aMethodInfo;
		//         }
		// 
		// 	    public override void DoAssemble() {
		//             EmitNotImplementedException(Assembler, GetServiceProvider(), "Sub_Ovf instruction not yet implemented",
		//                 mCurrentLabel, mCurrentMethodInfo, mCurrentOffset, mNextLabel);
		// 		}
		// 	}
		// }
		#endregion
	}
}
