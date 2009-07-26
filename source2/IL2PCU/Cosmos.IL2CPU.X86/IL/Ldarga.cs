using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldarga)]
	public class Ldarga: ILOp
	{
		public Ldarga(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// using System.Linq;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldarga)]
		// 	public class Ldarga: Op {
		// 		private int mAddress;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
		// 			mAddress = aMethodInfo.Arguments[aIndex].VirtualAddresses.First();
		// 		}
		//         public Ldarga(MethodInformation aMethodInfo, int aIndex, string aCurrentLabel, uint aCurrentOffset, string aNextLabel)
		// 			: base(null, aMethodInfo) {
		// 			SetArgIndex(aIndex, aMethodInfo);
		// 
		//             mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aCurrentOffset;
		// 		    mCurLabel = aCurrentLabel;
		//             mNextLabel = aNextLabel;
		// 		}
		// 
		// 		public Ldarga(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			if (aReader != null) {
		// 				SetArgIndex(aReader.OperandValueInt32, aMethodInfo);
		// 				//ParameterDefinition xParam = aReader.Operand as ParameterDefinition;
		// 				//if (xParam != null) {
		// 				//    SetArgIndex(xParam.Sequence - 1, aMethodInfo);
		// 				//}
		// 			}
		//             mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		//             new CPU.Push { DestinationReg = CPU.Registers.EBP };
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		//             new CPU.Push { DestinationValue = (uint)mAddress };
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		// 			Add(Assembler, GetServiceProvider(), mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
