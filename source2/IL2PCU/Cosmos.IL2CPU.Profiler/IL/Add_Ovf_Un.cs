using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Add_Ovf_Un)]
	public class Add_Ovf_Un: ILOpProfiler
	{



		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Add_Ovf_Un)]
		// 	public class Add_Ovf_Un: Add_Ovf {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Add_Ovf_Un(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 
		// 		public override void DoAssemble()
		// 		{
		//             EmitNotImplementedException(Assembler, GetServiceProvider(), "Add_Ovf_Un not yet implemented", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		// 
		// 			//AddWithOverflow(Assembler, false);
		// 		}
		// 	}
		// }
		#endregion
	}
}
