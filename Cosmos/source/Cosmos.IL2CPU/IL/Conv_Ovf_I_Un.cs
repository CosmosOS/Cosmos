using System;
using CPUx86 = Cosmos.Assembler.x86;
using Label = Cosmos.Assembler.Label;
namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_Ovf_I_Un)]
	public class Conv_Ovf_I_Un: ILOp
	{
		public Conv_Ovf_I_Un(Cosmos.Assembler.Assembler aAsmblr)
			:base(aAsmblr)
		{
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
			//TODO: What if the last ILOp in a method was Conv_Ovf_I_Un or an other?
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
			if(xSourceIsFloat)
				ThrowNotImplementedException("Conv_Ovf_I_Un throws an ArgumentException, because float is not implemented!");

			switch (xSourceSize)
			{
				case 1:
				case 2:
				case 4:
					break;
				case 8:
					{
						string NoOverflowLabel = GetLabel(aMethod, aOpCode) + "__NoOverflow";
						new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
						// EBX is high part and should be zero for unsigned, so we test it on zero
						{
							new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
							new CPUx86.Compare { DestinationReg = CPUx86.Registers.EBX, SourceValue = 0 };
							new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = NoOverflowLabel };
							ThrowNotImplementedException("Conv_Ovf_I_Un throws an overflow exception, which is not implemented!");
						}
						new Label(NoOverflowLabel);
						new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						break;
					}
				default:
					ThrowNotImplementedException("Conv_Ovf_I_Un not implemented for this size!");
					break;
			}
		}
    
		// using System;
		// using System.IO;
		// using CPU = Cosmos.Assembler.x86;
		// using Cosmos.IL2CPU.X86;
		// using CPUx86 = Cosmos.Assembler.x86;
		// 
		// namespace Cosmos.IL2CPU.IL.X86 {
		//     [Cosmos.Assembler.OpCode(OpCodeEnum.Conv_Ovf_I_Un)]
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