using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelem_R8)]
	public class Ldelem_R8: ILOp
	{
		public Ldelem_R8(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldelem_R8)]
		// 	public class Ldelem_R8: Op {
		//         public Ldelem_R8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			Ldelem_Ref.Assemble(Assembler, 8);
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
