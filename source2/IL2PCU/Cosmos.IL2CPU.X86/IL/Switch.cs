using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Switch)]
	public class Switch: ILOpX86
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Switch)]
		// 	public class Switch: ILOpX86 {
		// 		private string[] mLabels;
		// 		public Switch(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			uint[] xCases = aReader.OperandValueBranchLocations;
		// 			mLabels = new string[xCases.Length];
		// 			for (int i = 0; i < xCases.Length; i++) {
		// 			    mLabels[i] = GetInstructionLabel(xCases[i]);
		// 			}
		// 		}
		// 
		// 		public override void DoAssemble() {
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		// 			for(int i = 0; i < mLabels.Length; i++){
		//                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue =(uint)i };
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mLabels[i] };
		// 			}
		// 			Assembler.StackContents.Pop();
		// 		}
		// 	}
		// }
		#endregion
	}
}
