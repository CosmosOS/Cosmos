using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldarg)]
	public class Ldarg: ILOpProfiler
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldarg)]
		// 	public class Ldarg: ILOpProfiler {
		// 		private MethodInformation.Argument mArgument;
		// 		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
		// 			mArgument = aMethodInfo.Arguments[aIndex];
		// 		}
		// 
		// 		public Ldarg(MethodInformation aMethodInfo, int aIndex)
		// 			: base(null, aMethodInfo) {
		// 			SetArgIndex(aIndex, aMethodInfo);
		// 		}
		// 
		// 		public Ldarg(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			int xArgIndex;
		// 			if (aReader != null) {
		// 				xArgIndex = aReader.OperandValueInt32;
		// 				SetArgIndex(xArgIndex, aMethodInfo);
		// 				//ParameterDefinition xParam = aReader.Operand as ParameterDefinition;
		// 				//if (xParam != null) {
		// 				//    SetArgIndex(xParam.Sequence - 1, aMethodInfo);
		// 				//}
		// 			}
		// 		}
		// 
		// 		public override void DoAssemble() {
		// 			Ldarg(Assembler, mArgument);
		// 		}
		// 	}
		// }
		#endregion
	}
}
