using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldnull)]
	public class Ldnull: ILOpProfiler
	{



		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldnull)]
		// 	public class Ldnull: ILOpProfiler {
		// 		public Ldnull(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		//             new CPU.Push { DestinationValue = 0 };
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		// 		}
		// 	}
		// }
		#endregion
	}
}
