using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Conv_I1)]
	public class Conv_I1: ILOpX86
	{



		#region Old code
		// using System;
		// using System.IO;
		// using CPU = Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86
		// {
		//     [Cosmos.IL2CPU.OpCode(ILOp.Code.Conv_I1)]
		//     public class Conv_I1 : ILOpX86
		//     {
		//         private string mNextLabel;
		//         private string mCurLabel;
		//         private uint mCurOffset;
		//         private MethodInformation mMethodInformation;
		//         public Conv_I1(ILReader aReader, MethodInformation aMethodInfo)
		//             : base(aReader, aMethodInfo)
		//         {
		//             mMethodInformation = aMethodInfo;
		//             mCurOffset = aReader.Position;
		//             mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		//         }
		//         public override void DoAssemble()
		//         {
		//             StackContent xSource = Assembler.StackContents.Pop();
		//             if (xSource.IsFloat)
		//             {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I1: Floats not yet implemented", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		//             }
		//             switch (xSource.Size)
		//             {
		//                 case 1:
		//                     new CPUx86.Noop();
		//                     break;
		//                 case 2:
		//                 case 4:
		//                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                     new CPUx86.SignExtendAX { Size = 8 };
		//                     new CPUx86.SignExtendAX { Size = 16 };
		//                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		//                     break;
		//                 case 8:
		//                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                     new CPUx86.SignExtendAX { Size = 8 };
		//                     new CPUx86.SignExtendAX { Size = 16 };
		//                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		//                     break;
		//                 default:
		//                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                     return;
		//             }
		//             Assembler.StackContents.Push(new StackContent(1, true, false, true));
		//         }
		//     }
		// }
		#endregion
	}
}
