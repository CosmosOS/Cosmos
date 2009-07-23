using System;

namespace Cosmos.IL2CPU.ILOpCodes
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Shl)]
	public class Shl: ILOpCode
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Shl)]
		// 	public class Shl: ILOpCode {
		// 		public Shl(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX}; // shift amount
		// 			var xStackItem_ShiftAmount = Assembler.StackContents.Pop();
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX }; // value
		// 			var xStackItem_Value = Assembler.StackContents.Pop();
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceValue = 0 };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.AL };
		//             new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.CL };
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
		// 			Assembler.StackContents.Push(xStackItem_Value);
		// 		}
		// 	}
		// }
		#endregion
	}
}
