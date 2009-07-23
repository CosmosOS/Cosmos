using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldc_R8)]
	public class Ldc_R8: ILOpX86
	{



		#region Old code
		// using System;
		// using System.Linq;
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldc_R8)]
		// 	public class Ldc_R8: ILOpX86 {
		// 		private readonly Double mValue;
		// 		public Ldc_R8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mValue = aReader.OperandValueDouble;
		// 		}
		// 		public override void DoAssemble() {
		// 			byte[] xBytes = BitConverter.GetBytes(mValue);
		//             new CPU.Push { DestinationValue = BitConverter.ToUInt32(xBytes, 0) };
		//             new CPU.Push { DestinationValue = BitConverter.ToUInt32(xBytes, 4) };
		// 			Assembler.StackContents.Push(new StackContent(8, typeof(Double)));
		// 		}
		// 	}
		// }
		#endregion
	}
}
