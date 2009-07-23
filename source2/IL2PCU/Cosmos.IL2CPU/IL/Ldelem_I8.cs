using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldelem_I8)]
	public class Ldelem_I8: ILOpCode
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldelem_I8)]
		// 	public class Ldelem_I8: ILOpCode {
		//         
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
		//         //{
		//         //    Engine.RegisterType(typeof(long));
		//         //}
		// 
		//         public Ldelem_I8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		
		// 		public override void DoAssemble() {
		// 			Ldelem_Ref.Assemble(Assembler, 8);
		// 		}
		// 	}
		// }
		#endregion
	}
}
