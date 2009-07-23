using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldelem_R8)]
	public class Ldelem_R8: ILOpCode
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldelem_R8)]
		// 	public class Ldelem_R8: ILOpCode {
		//         public Ldelem_R8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			Ldelem_Ref.Assemble(Assembler, 8);
		// 		}
		// 	}
		// }
		#endregion
	}
}
