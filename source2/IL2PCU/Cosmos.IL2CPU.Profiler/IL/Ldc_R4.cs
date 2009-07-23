using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldc_R4)]
	public class Ldc_R4: ILOpProfiler
	{



		#region Old code
		// using System;
		// using System.Linq;
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldc_R4)]
		// 	public class Ldc_R4: ILOpProfiler {
		// 		private Single mValue;
		// 		public Ldc_R4(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mValue = aReader.OperandValueSingle;
		// 		}
		// 		public override void DoAssemble() {
		// 			new CPU.Push{DestinationValue=BitConverter.ToUInt32(BitConverter.GetBytes(mValue), 0)};
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(Single)));
		// 		}
		// 	}
		// }
		#endregion
	}
}
