using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_Ovf_I_Un)]
	public class Conv_Ovf_I_Un: ILOp
	{
		public Conv_Ovf_I_Un(Cosmos.Compiler.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      ThrowNotImplementedException("Conv_Ovf_I_Un not implemented!");
    }

    
		// using System;
		// using System.IO;
		// using CPU = Cosmos.Compiler.Assembler.X86;
		// using Cosmos.IL2CPU.X86;
		// using CPUx86 = Cosmos.Compiler.Assembler.X86;
		// 
		// namespace Cosmos.IL2CPU.IL.X86 {
		//     [OpCode(OpCodeEnum.Conv_Ovf_I_Un)]
		//     public class Conv_Ovf_I_Un : Op {
		//         private readonly string NextInstructionLabel;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 
		//         public Conv_Ovf_I_Un(ILReader aReader,
		//                              MethodInformation aMethodInfo)
		//             : base(aReader,
		//                    aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		//             NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
		//         }
		// 
		//         public override void DoAssemble() {
		//             var xSource = Assembler.Stack.Pop();
		//             switch (xSource.Size) {
		//                 case 1:
		//                 case 2:
		//                 case 4: {
		//                     break;
		//                 }
		//                 case 8: {
		//                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                         new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		//                     //new CPUx86.Pop(CPUx86.Registers_Old.EAX);
		//                     //new CPUx86.SignExtendAX(4);
		//                     ////all bits of EDX == sign (EAX)
		//                     //new CPUx86.Pop("EBX");
		//                     ////must be equal to EDX
		//                     //new CPUx86.Xor("EBX",
		//                     //               "EDX");
		//                     //new CPUx86.JumpIfZero(NextInstructionLabel);
		//                     ////equals
		//                     //new CPUx86.Interrupt(CPUx86.Interrupt.INTO);
		//                     break;
		//                 }
		//                 default:
		//                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_Ovf_I_Un: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                     break;
		//             }
		//             Assembler.Stack.Push(new StackContent(4,
		//                                                           true,
		//                                                           false,
		//                                                           false));
		//         }
		//     }
		// }
		
	}
}
