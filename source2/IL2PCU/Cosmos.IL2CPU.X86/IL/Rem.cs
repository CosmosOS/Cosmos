using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Rem)]
	public class Rem: ILOp
	{
		public Rem(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
        if( Assembler.Stack.Peek().IsFloat )
        {
            //EmitNotImplementedException( Assembler, GetServiceProvider(), "Rem: Float support not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
            throw new NotImplementedException();
        }
        var xStackItem = Assembler.Stack.Peek();
        int xSize = Math.Max( Assembler.Stack.Pop().Size, Assembler.Stack.Pop().Size );
        if( xSize > 4 )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };

            new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };

        }
        else
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };

            new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
        }
        Assembler.Stack.Push( xStackItem );
    }

    
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Rem)]
		// 	public class Rem: Op {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Rem(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			if (Assembler.Stack.Peek().IsFloat) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Rem: Float support not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			StackContent xStackItem = Assembler.Stack.Peek();
		// 			int xSize = Math.Max(Assembler.Stack.Pop().Size, Assembler.Stack.Pop().Size);
		// 			if (xSize > 4) {
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
		// 
		//                 new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
		// 
		// 			} else {
		// 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.ECX};
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
		// 
		//                 new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
		// 			}
		// 			Assembler.Stack.Push(xStackItem);
		// 		}
		// 	}
		// }
		
	}
}
