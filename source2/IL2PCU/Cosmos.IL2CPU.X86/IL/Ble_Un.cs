using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ble_Un)]
	public class Ble_Un: ILOpX86
	{
		public Ble_Un(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ble_Un)]
		// 	public class Ble_Un: Op {
		// 		public readonly string TargetLabel;
		// 		public readonly string CurInstructionLabel;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Ble_Un(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		// 			CurInstructionLabel = GetInstructionLabel(aReader);
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			StackContent xItem1 = Assembler.StackContents.Pop();
		// 			StackContent xItem2 = Assembler.StackContents.Pop();
		// 			int xSize = Math.Max(xItem1.Size, xItem2.Size);
		// 			var xIsFloat = xItem1.IsFloat || xItem2.IsFloat;
		// 			if (xIsFloat) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Ble_Un: Float support not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			if (xSize > 8) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Ble_Un: StackSize>8 not supported yet!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// 			if (xSize > 4)
		// 			{
		// 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		// 				//value2: EDX:EAX
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		// 				//value1: ECX:EBX
		//                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
		//                 new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
		// 				//result = value1 - value2
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.BelowOrEqual, DestinationLabel = TargetLabel };
		// 			} else
		// 			{
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; ;
		//                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.LessThanOrEqualTo, DestinationLabel = LabelFalse };
		//                 new CPUx86.Jump { DestinationLabel = LabelTrue };
		// 				new CPU.Label(LabelTrue);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Jump { DestinationLabel = TargetLabel };
		// 				new CPU.Label(LabelFalse);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		// 			}
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
