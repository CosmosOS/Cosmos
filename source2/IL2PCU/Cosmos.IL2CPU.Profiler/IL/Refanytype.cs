using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Refanytype)]
	public class Refanytype: ILOpProfiler
	{



		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Refanytype)]
		// 	public class Refanytype: ILOpProfiler {
		// 		public Refanytype(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// //			Pop("eax");
		// //			Pushd("[eax]");
		// 		}
		// 	}
		// }
		#endregion
	}
}
