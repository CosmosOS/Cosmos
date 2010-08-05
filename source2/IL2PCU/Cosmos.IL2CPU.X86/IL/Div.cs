using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Div )]
    public class Div : ILOp
    {
        public Div( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Assembler.Stack.Pop();
            if( xSize.Size == 8 )
            {
                //TODO: implement proper div support for 8byte values!
                if (xSize.IsFloat)
                {
                    new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                    new CPUx86.Add { SourceValue = 8, DestinationReg = Registers.ESP};
                    new CPUx86.x87.FloatDivide { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
                    new CPUx86.x87.FloatStoreAndPop { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                }
                else
                {
                    new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.IDivide { DestinationReg = CPUx86.Registers.ECX };
                    //new CPUx86.Push("0");
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
            }
            else
            {
                if (xSize.IsFloat)
                {
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.DivSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM1 };
                }
                else
                {
                    new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.IDivide { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
            }
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
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
        // 			var xSize = Assembler.Stack.Pop();
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

    }
}
