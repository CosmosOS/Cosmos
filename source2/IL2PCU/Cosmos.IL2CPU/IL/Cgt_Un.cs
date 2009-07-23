using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Cgt_Un)]
	public class Cgt_Un: ILOpCode
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using CPU = Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Cgt_Un)]
		// 	public class Cgt_Un: ILOpCode {
		// 		private readonly string NextInstructionLabel;
		//         private readonly string CurInstructionLabel;
		//         private uint mCurrentOffset;
		//         private MethodInformation mMethodInfo;
		//         public Cgt_Un(ILReader aReader, MethodInformation aMethodInfo)
		//             : base(aReader, aMethodInfo)
		//         {
		//             NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
		//             CurInstructionLabel = GetInstructionLabel(aReader);
		//             mMethodInfo = aMethodInfo;
		//             mCurrentOffset = aReader.Position;
		//         }
		// 
		// 	    public override void DoAssemble()
		// 		{
		// 			var xStackItem = Assembler.StackContents.Pop();
		//             if (xStackItem.IsFloat)
		//             {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Cgt_Un: Floats not yet supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
		//                 return;
		//             }
		//             if (xStackItem.Size > 8)
		//             {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Cgt_Un: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
		//                 return;
		//             }
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(bool)));
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// 			if (xStackItem.Size > 4)
		// 			{
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESI};
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 1 };
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI};
		// 				//esi = 1
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		//                 //value2: EDX:EAX
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 //value1: ECX:EBX
		//                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
		//                 new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
		// 				//result = value1 - value2
		// 				//new CPUx86.ConditionalMove(Condition.Above, "edi", "esi");
		//                 //new CPUx86.Push { DestinationReg = Registers.EDI };
		// 
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Above, DestinationLabel = LabelTrue };
		//                 new CPUx86.Push { DestinationValue = 0 };
		//                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
		// 
		// 				new CPU.Label(LabelTrue);
		// 				new CPUx86.Push{DestinationValue=1};
		// 
		// 			} else
		// 			{
		//                 new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
		//                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = LabelTrue };
		//                 new CPUx86.Jump { DestinationLabel = LabelFalse };
		//                 new CPU.Label(LabelTrue);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Push { DestinationValue = 1 };
		//                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
		//                 new CPU.Label(LabelFalse);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Push { DestinationValue = 0 };
		//                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
		// 			}
		// 		}
		// 	}
		// }
		#endregion
	}
}
