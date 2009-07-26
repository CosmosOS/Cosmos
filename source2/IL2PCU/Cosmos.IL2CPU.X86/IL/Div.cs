using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Div)]
	public class Div: ILOp
	{
		public Div(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      throw new Exception("TODO:");
    }

    #region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Div)]
		// 	public class Div: Op {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Div(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			var xSize = Assembler.StackContents.Pop();
		// 			if (xSize.IsFloat) {
		//                 EmitNotSupportedException(Assembler, GetServiceProvider(), "Floats not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			if (xSize.Size == 8) {
		// 				//TODO: implement proper div support for 8byte values!
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                 new CPUx86.IDivide { DestinationReg = CPUx86.Registers.ECX };
		// 				//new CPUx86.Push("0");
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 
		// 			} else {
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		// 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
		// 				new CPUx86.IDivide{DestinationReg=CPUx86.Registers.ECX};
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 			}
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
