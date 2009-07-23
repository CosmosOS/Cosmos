using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldc_I8)]
	public class Ldc_I8: ILOpCode
	{



		#region Old code
		// using System;
		// using System.IO;
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// using System.Diagnostics;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldc_I8)]
		// 	public class Ldc_I8: ILOpCode {
		// 		private readonly long mValue;
		// 		public Ldc_I8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo)
		// 		{
		// 			Debug.Assert(aReader.Operand.Length == 8);
		// 
		// 			ulong value = 0;
		// 			for (int i = 7; i >=0; i--)
		// 			{
		// 				value <<= 8;
		// 				value |= aReader.Operand[i];
		// 			}
		// 			mValue = (long)value;
		// 		}
		// 		public override void DoAssemble() {
		// 			string theValue = mValue.ToString("X16");
		//             new CPU.Push { DestinationValue = BitConverter.ToUInt32(BitConverter.GetBytes(mValue), 0) };
		//             new CPU.Push { DestinationValue = BitConverter.ToUInt32(BitConverter.GetBytes(mValue), 4) };
		// 			Assembler.StackContents.Push(new StackContent(8, typeof(long)));
		// 		}
		// 	}
		// }
		#endregion
	}
}
